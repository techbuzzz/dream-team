# Cases

> Глубокие дизайны для трёх ключевых use case-ов, которые ложатся на архитектуру v2 (form engine + rituals + notification + dashboard + AI digest) **без интеграций и без новых модулей**.

## Документы

- [skill-wheel.md](skill-wheel.md) — competency assessment, gap analysis, IDP через RoleProfile + ComputedGap
- [okr.md](okr.md) — Goals + KeyResults + CheckIns + linked-field aggregation (KRs auto-update из существующих submissions)
- [performance-review.md](performance-review.md) — multi-stage ritual (self → manager → calibration → delivery 1-1) + 9-box + action plan

## Общий тезис

Все три кейса — **не отдельные фичи**, а **разные режимы использования существующего form engine** + пара узкоспециализированных entities + 3-4 новых типа полей через field-registry.

**Что НЕ нужно:**
- Никаких интеграций с Jira / GitHub / Notion.
- Никаких внешних AI-сервисов (только self-hosted Ollama из v2).
- Никаких новых transport-каналов (только email + in-app SSE, как в v2).

**Что добавляется:**
- 8 новых entities:
  - Skill Wheel: `RoleProfile`, `CompetencyTarget`, `ComputedGap`
  - OKR: `Goal`, `KeyResult`, `CheckIn`, `LinkedFieldMapping`
  - Performance Review: `ReviewCycle`, `ReviewResponse`, `ReviewCalibration`, `ActionPlan`
- 4 новых DSL-типа полей (через field-registry):
  - `okr_summary` (prefill из user.assigned_goals)
  - `skill_wheel_summary` (prefill из user.computed_gaps)
  - `static_text` linked (readonly отображение submission от другого ритуала)
  - Расширение `matrix` типа (rowsSource + per-column prefill)
- 12 новых dashboard-виджетов

**Что переиспользуется из v1/v2:**
- `ProcessTemplate` + `FormVersion` + `ProcessInstance` + `Submission` (для всех трёх кейсов)
- `RitualSchedule` + `ScheduleException` (для cadence — quarterly review, weekly check-in, quarterly skill wheel)
- `Notification` + `NotificationDelivery` (для reminders — self deadline, manager deadline, calibration prep)
- SSE channel (для in-app уведомлений)
- Field-registry (для новых custom field types без форка движка)
- Form Renderer + Builder (для всех UI)

## Связь между кейсами

Три кейса **не изолированы** — они делят entities и поддерживают друг друга:

```
┌─────────────────────────────────────────────────────┐
│  Performance Review                                 │
│  ┌─────────────────────┐                            │
│  │ Self-Review форма   │ ← okr_summary (OKR)        │
│  │                     │ ← skill_wheel_summary     │
│  └─────────────────────┘   (Skill Wheel)            │
│  ┌─────────────────────┐                            │
│  │ Manager Review      │ ← skill_wheel_manager     │
│  │ форма               │   (Skill Wheel, linked)   │
│  └─────────────────────┘                            │
│  ┌─────────────────────┐                            │
│  │ Calibration (9-box) │ ← ComputedGap aggregated │
│  │                     │   (Skill Wheel)            │
│  └─────────────────────┘                            │
│  ┌─────────────────────┐                            │
│  │ Delivery 1-1        │ ← ActionPlan tracked      │
│  │                     │   from calibration        │
│  └─────────────────────┘                            │
└─────────────────────────────────────────────────────┘
            ↓ feeds into
┌─────────────────────────────────────────────────────┐
│  IDP (Individual Development Plan)                   │
│  Recommendations from ComputedGap (Skill Wheel)     │
│  + ActionPlan (Performance Review)                  │
│  + Goal cascade (OKR)                               │
└─────────────────────────────────────────────────────┘
            ↓ applied through
┌─────────────────────────────────────────────────────┐
│  Weekly OKR Check-in (per KR owner)                  │
│  Confidence + Pace + Blockers + Committed action     │
│  KR auto-updated via LinkedFieldMapping             │
└─────────────────────────────────────────────────────┘
```

**Конкретные cross-case flows:**

1. **Skill Wheel → OKR**: IDP target = OKR для next quarter. ActionPlan из Calibration становится Goal в next cycle.
2. **OKR → Performance Review**: Self-Review auto-pulls OKR progress за период через `okr_summary` field.
3. **Skill Wheel → Performance Review**: Manager Review auto-pulls competencies из self-review через `skill_wheel_manager` field.
4. **Performance Review → Skill Wheel**: 1-1 action plan → new IDP goals → next Skill Wheel review pre-fills "what did you commit to last cycle".

Эти cross-case flows реализуются **через prefill** (auto-population fields из других source-ов) и **через shared `ReviewCycle`** как anchor для cycle-based workflows.

## Dashboard-страницы

17 виджетов разбиты на 4 страницы:

| Страница | Виджеты |
|---|---|
| **Overview** | Team health pulse, Completion rate, Blockers feed, 1-1 overdue, Recent activity |
| **Skill Wheel** | Skill distribution, Self-manager agreement, Top 3 team gaps, Skill trajectory, Personal IDP |
| **OKR** | OKR tree, At-risk count, Confidence trend, Pace vs progress, Linked freshness |
| **Performance** | Calibration view, 9-box grid, Action plan status, Cycle timeline, Submission rate |

## Сводная таблица entities

| Entity | Кейс | Источник |
|---|---|---|
| `RoleProfile` | Skill Wheel | v2 (новое) |
| `CompetencyTarget` | Skill Wheel | v2 (новое) |
| `ComputedGap` | Skill Wheel | v2 (новое) |
| `Goal` | OKR | v2 (новое) |
| `KeyResult` | OKR | v2 (новое) |
| `CheckIn` | OKR | v2 (новое) |
| `LinkedFieldMapping` | OKR | v2 (новое) |
| `ReviewCycle` | Performance Review | v2 (новое) |
| `ReviewResponse` | Performance Review | v2 (новое) |
| `ReviewCalibration` | Performance Review | v2 (новое) |
| `ActionPlan` | Performance Review | v2 (новое) |
| `IdpRecommendation` | Cross-case (Skill Wheel + PR) | v2 (новое) |
| `ProcessTemplate` | Все | v1 |
| `FormVersion` | Все | v1 |
| `ProcessInstance` | Все | v1 |
| `Submission` | Все | v1 |
| `RitualSchedule` | Все | v2 |
| `Notification` | Все | v2 |
| `User` | Все | v1 |
| `Team` | Все | v1 |
| `TeamMembership` | Все | v1 |

**Итого: 12 новых entities для трёх кейсов + 9 переиспользуемых из v1/v2.**

## Связанные документы

- [../architecture.md](../architecture.md) — общая архитектура v2
- [../architecture-v1.md](../architecture-v1.md) — form engine v1
- [../processes.md](../processes.md) — каталог пресетов
- [../roadmap.md](../roadmap.md) — спринтовое планирование
