# MangaReader

Учебный проект: сайт для чтения манги на микросервисах (.NET 10, чистая архитектура).
Сейчас в проекте один сервис — **Catalog** (тайтлы и жанры).

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

База `manga_catalog`, таблицы и пять жанров создадутся автоматически.
API работает на http://localhost:5100. Запросы для проверки — в файле
`src/Services/Catalog/Catalog.Api/Catalog.Api.http` (открывается в Rider, VS, VS Code).

Дальше можно запускать просто так:

```cmd
dotnet run --project src\Services\Catalog\Catalog.Api
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
| `Catalog.Application` | Сценарии (`MangaService`), интерфейсы репозиториев и Unit of Work, DTO |
| `Catalog.Infrastructure` | EF Core + PostgreSQL, репозитории, Unit of Work, outbox, миграции |
| `Catalog.Api` | Контроллеры, обработка ошибок, запуск |

Зависимости направлены внутрь: Api → Application → Domain, Infrastructure → Application.

## Как устроено сохранение

1. Сущность меняется и записывает событие: `manga.Complete()` → `MangaCompleted`.
2. Сервис вызывает `unitOfWork.SaveChangesAsync()`.
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
