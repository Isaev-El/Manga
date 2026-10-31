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

## Если у вас другой пароль от Postgres

Поменяйте его в `src/Services/Catalog/Catalog.Api/appsettings.json`
(строка `ConnectionStrings:Catalog`) и не коммитьте это изменение.

## Изменили модель (сущности или конфигурации)?

Обязательно создайте миграцию, иначе приложение не запустится:

```cmd
dotnet ef migrations add НазваниеМиграции --project src\Services\Catalog\Catalog.Infrastructure --startup-project src\Services\Catalog\Catalog.Api --output-dir Persistence\Migrations
```

## Структура сервиса Catalog

| Проект | Что внутри |
|---|---|
| `Catalog.Domain` | Сущности `Manga`, `Genre`, бизнес-правила, доменные события |
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
