# MangaReader

Учебный проект: сайт для чтения манги на микросервисах (.NET 10, чистая архитектура).
Сервисы:

| Сервис | Что делает | Порт | База | Хранение |
|---|---|---|---|---|
| **Catalog** | Тайтлы, жанры, главы и страницы | 5100 | `manga_catalog` | Обычные таблицы + outbox |
| **Activity** | Списки пользователя, прогресс чтения, оценки | 5200 | `manga_activity` | **Event sourcing** |

## Что нужно установить

1. **.NET SDK 10** — https://dotnet.microsoft.com/download
2. **PostgreSQL** — https://www.postgresql.org/download/
   - при установке задайте пароль пользователя `postgres`: **postgres**
   - порт оставьте **5432**
   - в конце откроется Stack Builder — нажмите «Отмена», он не нужен

## Запуск

```cmd
git clone <ссылка на репозиторий>
cd MangaReader
run.cmd
```

На macOS / Linux: `./run.sh`

`run.cmd` запускает оба сервиса: Catalog в отдельном окне, Activity в текущем.
Базы `manga_catalog` и `manga_activity`, таблицы и пять жанров создадутся автоматически.

- Catalog: http://localhost:5100/scalar, запросы — `src/Services/Catalog/Catalog.Api/Catalog.Api.http`
- Activity: http://localhost:5200/scalar, запросы — `src/Services/Activity/Activity.Api/Activity.Api.http`

Activity обращается к Catalog по HTTP, поэтому Catalog должен быть запущен.

Дальше можно запускать по отдельности:

```cmd
dotnet run --project src\Services\Catalog\Catalog.Api
dotnet run --project src\Services\Activity\Activity.Api
```

## Запуск в Docker (без установки Postgres и .NET)

Нужен только Docker Desktop.

```cmd
docker compose up --build
```

Поднимутся два контейнера: Postgres и Catalog API.
API: http://localhost:5100/scalar. База из контейнера доступна на порту **5433**
(не 5432 — чтобы не конфликтовать с локально установленным Postgres).

Остановить: `Ctrl+C`, затем `docker compose down`.
Удалить и данные базы: `docker compose down -v`.

Если локально уже запущен Catalog через `run.cmd` — остановите его: порт 5100 будет занят.

## Если у вас другой пароль от Postgres

Поменяйте его в `src/Services/Catalog/Catalog.Api/appsettings.json`
(строка `ConnectionStrings:Catalog`) и не коммитьте это изменение.

## Изменили модель (сущности или конфигурации)?

Обязательно создайте миграцию, иначе приложение не запустится:

```cmd
dotnet ef migrations add НазваниеМиграции --project src\Services\Catalog\Catalog.Infrastructure --startup-project src\Services\Catalog\Catalog.Api --output-dir Migrations
```

## Структура сервиса Catalog

| Проект | Что внутри |
|---|---|
| `Catalog.Domain` | Сущности `Manga`, `Genre`, `Chapter` (со страницами), бизнес-правила, доменные события |
| `Catalog.Application` | Команды и запросы (MediatR): папка `Features`, интерфейсы репозиториев и Unit of Work, DTO |
| `Catalog.Infrastructure` | EF Core + PostgreSQL, репозитории, Unit of Work, outbox, миграции |
| `Catalog.Api` | Контроллеры, обработка ошибок, запуск |

Зависимости направлены внутрь: Api → Application → Domain, Infrastructure → Application.

## Как устроено сохранение

1. Сущность меняется и записывает событие: `manga.Complete()` → `MangaCompleted`.
2. Обработчик команды вызывает `unitOfWork.SaveChangesAsync()`.
3. Unit of Work собирает события со всех изменённых сущностей, кладёт их в таблицу
   `outbox_messages` и сохраняет всё **одной транзакцией**.

Позже события из outbox будут отправляться в RabbitMQ другим сервисам.

## Главы

Глава создаётся черновиком, в неё добавляются страницы, затем она публикуется.

| Правило | Ответ при нарушении |
|---|---|
| Номер главы > 0, можно дробный (10.5) | 400 |
| Номер уникален в пределах тайтла | 409 |
| Страницы можно менять только в черновике | 400 |
| Опубликовать главу без страниц нельзя | 400 |

При публикации глава поднимает событие `ChapterPublished`, и оно попадает в `outbox_messages`.

Новая миграция: `add-migration.cmd ИмяМиграции`.

## CI/CD (GitHub Actions)

Файл `.github/workflows/pipeline.yml`.

| Событие | Что происходит |
|---|---|
| Pull request в `main` или `test` | Сборка, проверка «не забыта ли миграция», smoke-тест API на настоящем Postgres |
| Merge в `test` | То же + Docker-образ `ghcr.io/isaev-el/manga-catalog:test` |
| Merge в `main` | То же + образ с тегами `:main` и `:latest` |

Pull request с красной проверкой не мерджится. Если упало:
- **Check that no migration is missing** — вы поменяли модель и не создали миграцию (`add-migration.cmd Имя`);
- **Smoke test** — API не запускается или эндпоинт отвечает не тем кодом; лог приложения в шаге **API log**.

Запустить опубликованный образ:

```cmd
docker pull ghcr.io/isaev-el/manga-catalog:test
```

## Команды, запросы и MediatR

Каждая операция — **один файл** в `Catalog.Application/Features/<Раздел>/`, в нём:

1. **Команда или запрос** — record с данными: `CreateMangaCommand : IRequest<Guid>`.
2. **Валидатор** (если нужен) — `AbstractValidator<CreateMangaCommand>`.
3. **Обработчик** — `IRequestHandler<CreateMangaCommand, Guid>`: сам сценарий.

Команда меняет данные (`Create…`, `Complete…`, `Publish…`), запрос только читает (`Get…`, `List…`).

Контроллер отправляет её через `ISender.Send(...)`, а MediatR сам находит обработчик.
Перед обработчиком запрос проходит через `ValidationBehavior` (`Behaviors/`):
он запускает все валидаторы и при ошибках возвращает 400, не доходя до обработчика.

```
Контроллер -> ISender.Send(command) -> ValidationBehavior -> Handler -> UnitOfWork
```

**Новая операция** = новый файл в `Features`. Регистрировать ничего не нужно:
обработчики и валидаторы находятся автоматически.

## Activity и event sourcing

Activity хранит не текущее состояние, а **историю действий** пользователя с тайтлом — как банковская выписка
вместо остатка на счёте.

**Таблица `events`** — главная, источник правды. Действия только добавляются, ничего не меняется и не удаляется:

| StreamId | Version | Type | Data |
|---|---|---|---|
| reading-ivan-7f3c… | 1 | AddedToList | `{"status":"Reading"}` |
| reading-ivan-7f3c… | 2 | ChapterRead | `{"chapterId":"…","number":1}` |
| reading-ivan-7f3c… | 3 | Rated | `{"score":9}` |

**Таблица `library_entries`** — проекция: готовый итог всех событий для быстрого показа «моих списков».
Её можно удалить и собрать заново из `events`.

**Как обрабатывается команда** (например, «прочитал главу»):

1. `ReadingEntryRepository.LoadAsync` читает события потока.
2. `ReadingEntry.Load` проигрывает их по порядку (`Apply`) — получается текущее состояние.
3. Метод агрегата (`MarkChapterRead`) проверяет правила и создаёт новое событие.
4. `SaveAsync` дописывает событие в `events` и обновляет `library_entries` — одной транзакцией.
5. Если кто-то успел записать событие с тем же номером, уникальный индекс `(StreamId, Version)` не даст
   сохранить, и API вернёт **409**.

| Где | Файл |
|---|---|
| Агрегат и правила | `Activity.Domain/ReadingEntry.cs` |
| События | `Activity.Domain/Events/ReadingEvents.cs` |
| Хранилище событий | `Activity.Infrastructure/Repositories/ReadingEntryRepository.cs` |
| Проекция | `Activity.Infrastructure/Projections/LibraryEntryProjector.cs` |
| Запросы на чтение | `Activity.Infrastructure/ReadModel/LibraryReadModel.cs` |

Пользователь пока передаётся заголовком `X-User-Id` — когда появится сервис Identity, его заменит JWT.

Новая миграция для Activity: `add-migration.cmd ИмяМиграции Activity`.
