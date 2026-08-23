# Processes & Presets

> Каталог коробочных процессов (пресетов) DreamTeam. Каждый пресет — это `FormVersion` + ProcessTemplate + рекомендованный cadence и audience. Лид копирует, настраивает, публикует.

## Конвенция именования

- **Process Type** (enum): DAILY_SYNC, ONE_ON_ONE, RETRO, PLANNING, SKILL_WHEEL, OKR_REVIEW, CUSTOM
- **Audience Type**: `team` (вся команда) | `members` (lead + member pair) | `roles` (по ролям) | `custom` (явный список)
- **Cadence**: cron-строка + timezone
- **Default duration**: 15-60 минут

## Каталог пресетов

### 1-1 Weekly Check-in

| Параметр | Значение |
|---|---|
| Process Type | ONE_ON_ONE |
| Audience | members (lead + 1 member) |
| Cadence | `0 10 * * 1` (каждый понедельник 10:00 local) |
| Duration | 30 мин |
| Reminder | за 60 мин |
| Tags | growth, blocker-surfacing, alignment |

**Структура формы (3 страницы):**

```json
{
  "pages": [
    {
      "id": "checkin",
      "title": "Check-in",
      "elements": [
        { "id": "energy", "type": "rating", "label": "Energy this week", "scale": 5, "required": true },
        { "id": "mood", "type": "longtext", "label": "How are you, really?" },
        { "id": "blockers", "type": "longtext", "label": "What's slowing you down?" },
        { "id": "focus", "type": "longtext", "label": "What's your focus this week?" }
      ]
    },
    {
      "id": "growth",
      "title": "Growth",
      "elements": [
        { "id": "skill_areas", "type": "skill_wheel", "label": "Self-assessment (rotating)",
          "categories": ["Technical depth", "Product thinking", "Collaboration", "Delivery"],
          "scale": 5
        },
        { "id": "career", "type": "longtext", "label": "Where do you want to be in 12 months?" },
        { "id": "manager_support", "type": "longtext", "label": "How can I help this week?",
          "visibleIf": "data.energy < 3" }
      ]
    },
    {
      "id": "action_items",
      "title": "Action items",
      "elements": [
        { "id": "actions", "type": "repeater", "label": "What are we committing to?",
          "template": [
            { "id": "action", "type": "longtext", "label": "Action" },
            { "id": "owner", "type": "select", "label": "Owner", "options": ["member", "lead"] },
            { "id": "due", "type": "date", "label": "Due" }
          ]
        }
      ]
    }
  ]
}
```

**Notes:**
- Energy < 3 → trigger "manager_support" поле (visibleIf condition)
- Skill Wheel ротация: раз в 4 недели (настраивается на уровне ProcessTemplate)
- Action items автоматически становятся tasks в next week ritual

### Daily Standup

| Параметр | Значение |
|---|---|
| Process Type | DAILY_SYNC |
| Audience | team (вся команда) |
| Cadence | `0 10 * * 1-5` (Mon-Fri 10:00) |
| Duration | 15 мин |
| Reminder | за 10 мин |
| Tags | sync, blocker-surfacing |

**Структура формы:**

```json
{
  "pages": [
    {
      "id": "standup",
      "title": "Daily sync",
      "elements": [
        { "id": "yesterday", "type": "longtext", "label": "What did you do yesterday?" },
        { "id": "today", "type": "longtext", "label": "What will you do today?" },
        { "id": "blockers", "type": "longtext", "label": "Any blockers?" },
        { "id": "mood", "type": "rating", "label": "Energy today", "scale": 5 }
      ]
    }
  ]
}
```

**Notes:**
- Skip weekends: `1-5` в cron
- Reminder 10 мин (а не 60) — daily standup, не нужен большой lead
- Auto-rollup в dashboard (per-team aggregation)

### Sprint Retro

| Параметр | Значение |
|---|---|
| Process Type | RETRO |
| Audience | team |
| Cadence | `0 16 * * 5#2` (каждый 2-й четверг в 16:00 — конец спринта) |
| Duration | 45 мин |
| Reminder | за 24 часа + за 60 мин |
| Tags | retrospective, process-improvement |

**Структура формы (3-колонка Start/Stop/Continue + action items):**

```json
{
  "pages": [
    {
      "id": "observations",
      "title": "What happened?",
      "elements": [
        { "id": "start", "type": "longtext", "label": "Start — what should we begin doing?" },
        { "id": "stop", "type": "longtext", "label": "Stop — what should we stop doing?" },
        { "id": "continue", "type": "longtext", "label": "Continue — what's working?" }
      ]
    },
    {
      "id": "vote",
      "title": "Vote on top items",
      "elements": [
        { "id": "top_three", "type": "rank", "label": "Pick top 3 to discuss",
          "sourceFrom": "submissions_of_previous_page",
          "maxItems": 3
        }
      ]
    },
    {
      "id": "actions",
      "title": "Action items",
      "elements": [
        { "id": "actions", "type": "repeater", "label": "What are we trying next sprint?",
          "template": [
            { "id": "action", "type": "longtext" },
            { "id": "owner", "type": "select", "options": ["team_lead", "team_member"] },
            { "id": "due", "type": "date" }
          ]
        }
      ]
    }
  ]
}
```

**Notes:**
- Page 2 (Vote) — это ranking-поле, источник — submissions предыдущей страницы (требует custom field type "rank")
- Auto-summarized в weekly digest

### Skill Wheel Review (Quarterly)

| Параметр | Значение |
|---|---|
| Process Type | SKILL_WHEEL |
| Audience | members (self + lead review) |
| Cadence | `0 10 1 */3 *` (каждый квартал, 1-й день) |
| Duration | 60 мин (1-1 для calibration) |
| Reminder | за 7 дней + за 24 часа |
| Tags | growth, calibration, performance |

**Структура формы (custom field type skill_wheel):**

```json
{
  "pages": [
    {
      "id": "self",
      "title": "Self-assessment",
      "elements": [
        { "id": "skill_areas", "type": "skill_wheel", "label": "Rate yourself 1-5",
          "categories": [
            "Technical depth",
            "Product thinking",
            "Collaboration",
            "Delivery"
          ],
          "scale": 5,
          "evidenceRequired": true
        }
      ]
    },
    {
      "id": "evidence",
      "title": "Evidence per area",
      "elements": [
        { "id": "technical_evidence", "type": "longtext",
          "label": "Recent work that demonstrates your technical depth",
          "visibleIf": "data.skill_areas['Technical depth'] >= 3" },
        { "id": "product_evidence", "type": "longtext",
          "label": "Product thinking evidence",
          "visibleIf": "data.skill_areas['Product thinking'] >= 3" }
      ]
    }
  ]
}
```

**Notes:**
- Два submission на инстанс: self-review + lead-review, side-by-side в dashboard
- Auto-generated growth trajectory graph по quarter

### OKR Check-in (Weekly)

| Параметр | Значение |
|---|---|
| Process Type | OKR_REVIEW |
| Audience | team |
| Cadence | `0 9 * * 1` (каждый понедельник 09:00) |
| Duration | 30 мин |
| Reminder | за 30 мин |
| Tags | goals, alignment, accountability |

**Структура формы (per-KR matrix):**

```json
{
  "pages": [
    {
      "id": "kr_update",
      "title": "Per-KR update",
      "elements": [
        { "id": "kr_progress", "type": "matrix", "label": "Update per key result",
          "rowsSource": "team.active_goals.key_results",
          "columns": [
            { "id": "current", "label": "Current %", "type": "number", "min": 0, "max": 100 },
            { "id": "status", "label": "Status", "type": "select",
              "options": ["on-track", "behind", "at-risk"] },
            { "id": "confidence", "label": "Confidence", "type": "rating", "scale": 10 },
            { "id": "moved", "label": "What moved it?", "type": "longtext" }
          ]
        }
      ]
    },
    {
      "id": "blockers",
      "title": "Blockers & next week",
      "elements": [
        { "id": "blockers", "type": "longtext", "label": "What's blocking?" },
        { "id": "next_week", "type": "longtext", "label": "Your focus next week" }
      ]
    }
  ]
}
```

**Notes:**
- `rowsSource: "team.active_goals.key_results"` — динамический источник строк (требует custom field type "matrix-with-source")
- 3-stato цвета (on-track/behind/at-risk) → dashboard виджет "OKR at-risk count"
- Confidence scale 1-10 → trend graph

## Custom presets

Лиды могут создавать свои пресеты через Form Builder. Типичные примеры:

- **Monthly Health Check** — team survey (1-5 по culture/process/satisfaction)
- **Onboarding 30/60/90** — members + checklist
- **Demo Day Prep** — team (Friday before demo)
- **Incident Postmortem** — roles (engineering leads only)
- **Cross-team Sync** — custom (specific list of people)

## Cadence patterns

Стандартные cron-строки для копирования в UI:

| Когда | Cron |
|---|---|
| Каждый понедельник 10:00 | `0 10 * * 1` |
| Каждый вторник 14:30 | `30 14 * * 2` |
| Каждые 2 недели в четверг | `0 10 * * 4/2` |
| 1-й день каждого месяца | `0 10 1 * *` |
| Последний рабочий день месяца | `0 17 * * 5L` |
| Каждый будний день 09:00 | `0 9 * * 1-5` |
| Каждые 3 месяца (квартал) | `0 10 1 */3 *` |

## Лучшие практики

1. **Не больше 5-7 вопросов в daily standup** — иначе люди забивают.
2. **1-1 — всегда пара (lead + member)**, не вся команда.
3. **Retro — не чаще 2 недель**, иначе нет материала.
4. **Skill Wheel — раз в квартал**, не чаще (иначе превращается в performance review).
5. **OKR check-in — еженедельно, но только если есть активные OKR**.
6. **Audience filter: только team players** (лид выбирает руками) — для risk-аудита или escalation.
7. **Reminder lead зависит от длительности**: 1-1 (30+ мин) → 60 мин, daily (15 мин) → 10 мин, retro (45+ мин) → 24 часа.

## Дальнейшее расширение

Пресеты добавляются по запросу community. Roadmap: onboarding 30/60/90, demo day prep, incident postmortem, cross-team sync, monthly health check.

В v4 возможны «presets marketplace» с пресетами от community (с модерацией).
