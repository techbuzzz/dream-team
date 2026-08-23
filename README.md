# DreamTeam

> **Form-first rituals platform для ТимЛида / ТехЛида / СквадЛида.**
> Сделал форму один раз — система сама рассылает её команде по расписанию, агрегирует ответы, рисует дашборд и формирует weekly digest через локальный LLM.

## Что это

DreamTeam — это «армейский нож» для тимлида. В основе лежит **form engine** как runtime-среда: 1-1, daily, retro, OKR check-in, skill wheel review — это не отдельные страницы, которые разработчик пишет месяцами, а **preset-формы**, которые лид копирует и настраивает под себя за минуты.

Поверх form engine — три модуля:
- **Ritual Scheduler** — расписание с cron + timezone + audience, авто-инстансы
- **Notification Pipeline** — email + in-app + retry, templates в БД
- **Team Dashboard** — 7 виджетов поверх submissions
- **AI Digest** (киллер-фича) — weekly summary через self-hosted LLM

## Стек

| Слой | Технология |
|---|---|
| Backend | .NET 10 (LTS до 14.11.2028) + Minimal APIs + EF Core 10 |
| Frontend | Nuxt 4 (стабилен с 16.07.2025) + Vue 3 + VeeValidate + Zod |
| Database | PostgreSQL 16 + JSONB |
| Auth | ASP.NET Core Identity + JWT + refresh rotation |
| Scheduler | Hangfire |
| Email | MailKit + FluentEmail |
| Realtime | SSE (MVP) → SignalR + Redis (v4) |
| Attachments | MinIO (S3-compatible) |
| LLM | Ollama (llama3.1:8b) → vLLM (prod scale) |
| Background jobs | Hangfire |

## Структура репозитория

```
dreamteam/
├── apps/
│   ├── api/         # .NET 10 backend (Minimal APIs + EF Core)
│   └── web/         # Nuxt 4 frontend
├── packages/
│   └── shared/      # Form DSL + Zod schemas (TS, для client+server)
├── infra/
│   ├── docker-compose.yml          # api + web + postgres + minio + mailhog
│   ├── docker-compose.ollama.yml   # + Ollama (optional, для AI digest)
│   └── Makefile
└── docs/
    ├── README.md           # ← вы здесь
    ├── architecture.md     # v2: полная архитектура (form engine + rituals + notif + dashboard + digest)
    ├── architecture-v1.md  # v1: только form engine (зафиксированная история)
    ├── roadmap.md          # Спринтовое планирование, MVP-1..v4
    └── processes.md        # Каталог пресетов процессов (1-1, Daily, Retro, Skill Wheel, OKR)
```

## Документация

- **[`docs/architecture.md`](docs/architecture.md)** — v2 архитектура (читай первым)
- [`docs/architecture-v1.md`](docs/architecture-v1.md) — v1 form engine (исторический)
- [`docs/roadmap.md`](docs/roadmap.md) — спринтовое планирование, MVP-cut, метрики
- [`docs/processes.md`](docs/processes.md) — каталог пресетов

## Быстрый старт (когда код будет готов)

```bash
# 1. Клонировать
git clone <repo>
cd dreamteam

# 2. Запустить инфраструктуру
cd infra
docker compose -f docker-compose.yml -f docker-compose.dev.yml up -d

# 3. Запустить миграции + seed
cd ../apps/api
dotnet ef database update
# seed создаёт 2 команды, 6 пользователей, дефолтные пресеты

# 4. Запустить API
dotnet run --project DreamTeam.Api
# → http://localhost:3001

# 5. В другом терминале — frontend
cd ../web
pnpm install
pnpm dev
# → http://localhost:3000

# 6. (Опционально) AI digest через локальный LLM
cd ../../infra
docker compose -f docker-compose.ollama.yml up -d
# Ollama тянет llama3.1:8b-instruct при первом старте
```

## Архитектурные принципы

1. **Form is data, not code** — форма = JSON, рендерер generic.
2. **Snapshot-on-publish** — `ProcessInstance` всегда указывает на конкретную `FormVersion`.
3. **Append-only submissions** — исправления = новые строки, никогда `UPDATE`.
4. **Field registry** — новые типы полей через регистрацию компонента + схемы.
5. **Preset-as-form** — 1-1, Daily, Retro, Skill Wheel, OKR = копируемые пресеты.
6. **Schedules are first-class** — `RitualSchedule` отделён от `ProcessTemplate`.
7. **Notifications are intents, not sends** — `Notification` отделён от `NotificationDelivery`.
8. **AI is pluggable, data is sovereign** — digest через `IDigestLlm` интерфейс, данные не покидают БД.

## Позиционирование

| Конкурент | Что делает | Наш дифференциатор |
|---|---|---|
| Standuply / Geekbot | Standup-боты в Slack/Teams | Web-app, не бот. Self-hostable. AI digest |
| monday.com Daily Standup | Standup внутри monday | Form-first, любая структура, не привязан к JIRA |
| Typeform / Tally / Form.io | Form-builder, нет rituals | Scheduling + audience + digest. Не просто форма |
| Range / Lattice / 15Five | Performance + rituals | Army knife, не performance tool |
| Notion + Zapier | DIY rituals | Коробочное решение с правильными дефолтами |

**Одним предложением:** *«Army knife для ТимЛида: сделал форму один раз — система сама рассылает её команде по расписанию, агрегирует ответы, рисует дашборд и формирует weekly digest через локальный LLM»*.

## Roadmap

- **MVP-1** (4-6 нед) — Form engine + 1-1 preset + auth + builder/renderer
- **MVP-2** (+6-8 нед) — Ritual Scheduler + Notification Pipeline + Team Dashboard + AI Digest
- **MVP-3** (+4 нед) — Polish: visual conditional logic, computed fields, calendar integration, PWA, multi-lang
- **v4** (+3+ мес) — Enterprise: SignalR cluster, OpenIddict, multi-tenant, Slack/Teams, vLLM

См. [`docs/roadmap.md`](docs/roadmap.md) для деталей.

## Лицензия

TBD (предположительно AGPLv3 + commercial dual-license, по модели SurveyJS / Form.io).

## Контрибуция

TBD.
