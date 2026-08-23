# DreamTeam Roadmap

> Спринтовое планирование для v1 + v2. Непрерывный бэклог с MVP-cut, epic-структурой, метриками готовности.

## Период

- **Старт:** Q3 2026
- **MVP-1 готов:** конец Q3 2026 (~6 недель)
- **MVP-2 готов:** Q4 2026 (~14 недель от старта)
- **MVP-3 готов:** Q1 2027 (~18 недель)
- **v4 (Enterprise):** Q2-Q3 2027

## Epic-структура

```
EPIC-1: Form Engine Foundation        [MVP-1]
  ├─ E1.1 Backend: .NET 10 + EF Core + Postgres
  ├─ E1.2 Auth: ASP.NET Identity + JWT + refresh
  ├─ E1.3 Form DSL + Zod-builder
  ├─ E1.4 Form Renderer (Nuxt 4 + VeeValidate)
  ├─ E1.5 Form Builder (drag-and-drop + preview)
  └─ E1.6 Preset: 1-1 (эталон)

EPIC-2: Ritual Scheduling & Audience [MVP-2]
  ├─ E2.1 RitualSchedule entity + cron
  ├─ E2.2 Hangfire integration
  ├─ E2.3 Instance Generator Job
  ├─ E2.4 Reminder Dispatcher
  ├─ E2.5 ScheduleException (holidays)
  └─ E2.6 Audience snapshot + access checks

EPIC-3: Notification Pipeline          [MVP-2]
  ├─ E3.1 Notification + NotificationDelivery entities
  ├─ E3.2 MailKit + FluentEmail integration
  ├─ E3.3 Email templates registry
  ├─ E3.4 Retry policy (Polly)
  ├─ E3.5 SSE channel (in-app)
  └─ E3.6 Notification preferences per user

EPIC-4: Team Dashboard                [MVP-2]
  ├─ E4.1 Dashboard endpoints (read-only projections)
  ├─ E4.2 Widget: Team health pulse
  ├─ E4.3 Widget: Completion rate
  ├─ E4.4 Widget: Blockers feed
  ├─ E4.5 Widget: 1-1 overdue
  ├─ E4.6 Widget: Skill wheel drift
  ├─ E4.7 Widget: OKR at-risk count
  └─ E4.8 Drilldown views (per-player, per-ritual)

EPIC-5: Preset Library Expansion      [MVP-2]
  ├─ E5.1 Daily Sync preset
  ├─ E5.2 Sprint Retro preset (Start/Stop/Continue)
  ├─ E5.3 Skill Wheel Review preset
  ├─ E5.4 OKR Check-in preset
  └─ E5.5 Preset versioning + propagation

EPIC-6: AI Digest (Killer Feature)    [MVP-2]
  ├─ E6.1 Ollama integration (OpenAI-compatible client)
  ├─ E6.2 DigestRun + DigestArtifact entities
  ├─ E6.3 Aggregation pipeline
  ├─ E6.4 Prompt engineering + few-shot examples
  ├─ E6.5 Validation (no hallucinated user-ids)
  ├─ E6.6 Weekly digest cron + delivery
  └─ E6.7 Cost tracking (input/output tokens)

EPIC-7: Polish & UX                   [MVP-3]
  ├─ E7.1 Visual conditional logic editor
  ├─ E7.2 Computed fields + server-side computation
  ├─ E7.3 Calendar integration (Google / Outlook)
  ├─ E7.4 Mobile PWA
  ├─ E7.5 Multi-language digest (ru/en)
  └─ E7.6 Builder UX improvements (keyboard nav, undo/redo polish)

EPIC-8: Enterprise Scale              [v4]
  ├─ E8.1 SignalR + Redis backplane
  ├─ E8.2 OpenIddict (OIDC / SSO)
  ├─ E8.3 Multi-tenant / multi-org
  ├─ E8.4 Slack / Teams integration (alternative to in-app)
  ├─ E8.5 vLLM migration path
  └─ E8.6 Audit log + compliance reports
```

## MVP-1: Form Engine Foundation (Sprint 1-2, 4-6 недель)

**Definition of Done:**
- Backend: .NET 10 Minimal APIs, EF Core 10, Postgres JSONB, JWT + refresh rotation
- 8 entities: User, Team, TeamMembership, ProcessTemplate, FormVersion, ProcessInstance, Submission (+ Answer опционально)
- 12 базовых типов полей + 1 extension (Skill Wheel)
- 1 preset: 1-1
- Builder: drag-and-drop, live preview, property panel, undo/redo, save draft, publish
- Renderer: schema → Zod → Vue, autosave drafts
- Auth: login/logout/refresh, 4 роли, multi-team для PM
- Tests: vitest + supertest, ≥20 backend tests, ≥6 frontend tests
- Docker compose: api + web + postgres, smoke test green

**Sprint 1 (2 недели):**
- E1.1 (1-2 дня): .NET 10 solution, EF Core 10, Postgres JSONB, миграции
- E1.2 (1 день): ASP.NET Core Identity + JWT + refresh rotation + RBAC policies
- E1.3 (1-2 дня): Form DSL, Zod-builder, JSON Schema
- E1.6 (1 день): 1-1 preset как эталон (тестовый seed)

**Sprint 2 (2 недели):**
- E1.4 (1 неделя): Nuxt 4 + VeeValidate + Zod, FormRenderer, FieldRegistry, 12 базовых полей
- E1.5 (1 неделя): Form Builder, drag-and-drop, live preview, undo/redo, publish flow
- Tests + Docker compose + smoke (1-2 дня)

**Exit criteria:**
- [ ] `docker compose up` → http://localhost:3000 (web) и http://localhost:3001 (api)
- [ ] Login как PM → создать ProcessTemplate → опубликовать FormVersion → открыть форму → заполнить → submission сохранён
- [ ] Audit: form_versions + submissions immutable
- [ ] CI: lint + typecheck + test green на PR

## MVP-2: Rituals + Notifications + Dashboard + Digest (Sprint 3-6, 6-8 недель)

**Definition of Done:**
- Ritual Scheduler с Hangfire, cron, timezone, audience
- Notification Pipeline (email + in-app SSE), retry, templates
- Team Dashboard с 7 виджетами
- 4 новых пресета: Daily, Retro, Skill Wheel, OKR
- AI Digest через Ollama, weekly cadence
- Docker compose: + mailhog + ollama (optional)

**Sprint 3 (2 недели):**
- E2.1, E2.5, E2.6: RitualSchedule entity, audience snapshot, миграции
- E3.1, E3.2, E3.3: Notification entities, MailKit integration, email templates
- E3.4: Retry policy через Polly

**Sprint 4 (2 недели):**
- E2.2: Hangfire integration, dashboard
- E2.3, E2.4: Instance Generator Job, Reminder Dispatcher
- E3.5: SSE channel для in-app уведомлений
- E5.1, E5.2: Daily Sync, Sprint Retro пресеты

**Sprint 5 (2 недели):**
- E5.3, E5.4: Skill Wheel, OKR Check-in пресеты
- E4.1, E4.2, E4.3, E4.4: Dashboard endpoints + 4 виджета
- E4.8: Drilldown views

**Sprint 6 (2 недели):**
- E4.5, E4.6, E4.7: оставшиеся 3 виджета
- E6.1, E6.2, E6.3: Ollama integration, DigestRun entities, aggregation pipeline
- E6.4, E6.5, E6.6, E6.7: prompt engineering, validation, weekly cron, cost tracking
- Tests + polish

**Exit criteria:**
- [ ] PM создаёт 1-1 schedule с cadence "каждый понедельник 10:00 MSK"
- [ ] Hangfire генерирует ProcessInstance на следующий понедельник
- [ ] За 60 мин до scheduledAt у Маши в in-app + email приходит invitation
- [ ] Маша заполняет форму → submission сохранён
- [ ] В пятницу 17:00 — лид получает weekly digest с TL;DR / Highlights / Concerns
- [ ] Dashboard показывает completion rate за неделю
- [ ] Все 4 новых пресета работают end-to-end

## MVP-3: Polish & UX (Sprint 7-8, 4 недели)

- E7.1: Visual conditional logic editor (rule-builder)
- E7.2: Computed fields + server-side computation engine (TS-движок, переиспользуется client+server)
- E7.3: Calendar integration (Google Calendar / Outlook) — iCal export для ритуалов
- E7.4: Mobile PWA (service worker, push notifications)
- E7.5: Multi-language digest (ru/en prompts)
- E7.6: Builder UX improvements (keyboard nav, undo/redo polish, mobile-friendly canvas)

**Exit criteria:**
- [ ] Лид может визуально нарисовать "if energy < 3 → show manager_support"
- [ ] Computed field "overall_skill_score" пересчитывается автоматически
- [ ] .ics файл с ритуалом импортируется в Google Calendar / Outlook
- [ ] PWA устанавливается на iOS/Android
- [ ] Digest генерируется на русском для ru-локализованных команд

## v4: Enterprise Scale (Sprint 9+, 3+ месяца)

- E8.1: SignalR + Redis backplane для multi-instance deployment
- E8.2: OpenIddict (OIDC / SSO) для корпоративных IdP
- E8.3: Multi-tenant / multi-org (отдельные пространства с собственными admin)
- E8.4: Slack / Teams integration как альтернатива in-app уведомлениям
- E8.5: vLLM migration path (production scale, GPU)
- E8.6: Audit log + compliance reports (SOC2, GDPR)

## Технический долг и метрики

**Per-sprint:**
- Code coverage: backend ≥70%, frontend ≥60%
- Lighthouse score: ≥85 на ключевых страницах
- Bundle size: web <500KB gzipped
- API p95 latency: <200ms на dashboard endpoints

**Per-milestone:**
- Smoke test (curl + Playwright) green
- Docker compose разворачивается за <2 минут
- Backup-restore procedure задокументирован
- Self-host checklist: hardware requirements, network ports, TLS

## Открытые вопросы для продукт-менеджера

- **Q-PM-1**: Pricing модель для SaaS-режима (если будет)? Self-hosted остаётся бесплатным?
- **Q-PM-2**: Mobile native (iOS/Android) или PWA достаточно? PWA дешевле, native — лучше UX.
- **Q-PM-3**: AI digest — бесплатная фича или premium tier? Compute cost может быть значимым.
- **Q-PM-4**: Публичные формы (cross-team surveys) — в каком milestone? MVP-3 или v4?
- **Q-PM-5**: Готовы ли к тому, что первый MVP без Slack/Teams? Изначально только in-app + email.

## Команда (предположение)

- 1 Tech Lead (architecture, code review, .NET + Nuxt)
- 1 Backend Engineer (.NET 10, Postgres, Hangfire)
- 1 Frontend Engineer (Nuxt 4, Vue 3, UX)
- 0.5 DevOps (docker, CI/CD, мониторинг)
- 0.25 Designer (UX для builder и dashboard, можно контрактор)

**Бюджет:** ~2.75 FTE × 6-8 недель = ~16-22 person-weeks на MVP-1+MVP-2.

## Метрики успеха

**После MVP-1 (4-6 недель):**
- 5-10 internal teams используют форму 1-1
- ≥80% retention (команды продолжают использовать после первого месяца)
- Время создания новой 1-1-сессии <2 минут (без разработчика)

**После MVP-2 (14 недель):**
- 20+ teams, 5+ presets используются регулярно
- Daily digest открывается лидом в ≥60% случаев
- Dashboard — самый посещаемый раздел для PM/Delivery

**После MVP-3 (18 недель):**
- NPS ≥40 от лидов
- Time-to-first-ritual для новой команды <30 минут
- ≥70% retention на 3-й месяц
