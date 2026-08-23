# Performance Review Case

> Глубокий дизайн Performance Review в рамках архитектуры v2: multi-stage ritual, calibration meeting, 9-box grid, action plan delivery. Без 360 (это v4), без compensation linking.

## Что это

Performance Review — это **multi-stage процесс**, а не одна форма. Из research: Performance Review и 360 — **разные инструменты** с разными вопросами:
- Performance Review: "hit the targets?" (outcomes, goals)
- 360: "how does this person lead?" (behaviors, competencies)

Мы разделяем их явно. В MVP-2 — только Performance Review. 360 — v4 (как расширение Skill Wheel Review, см. [skill-wheel.md](skill-wheel.md)).

## Ключевые принципы (из research)

- **Multi-stage**: self-review → manager review → calibration → delivery. Каждый этап — отдельный ritual schedule.
- **Self + manager = minimum defensible review.** Self-only = opinion, не data.
- **9-box calibration** — не self-rating aggregate, а **manager cross-calibration** meeting.
- **Action plan per person** — обязателен. Без follow-through вся затея — labeling exercise.
- **Box label confidential** — **НЕ сообщается** reviewee verbatim. Manager структурирует разговор сам.
- **Demographic pattern check** — обязателен на calibration. Если все "high potential" одного возраста/пола/бэкграунда — это calibration finding, не coincidence.

## Доменные entities (новые)

```csharp
public class ReviewCycle
{
    public Guid Id { get; set; }
    public string Name { get; set; }              // "Q3 2026 Performance Review"
    public Guid TeamId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime SelfDeadline { get; set; }
    public DateTime ManagerDeadline { get; set; }
    public DateTime CalibrationDate { get; set; } // when managers meet
    public ReviewStatus Status { get; set; }      // Setup | SelfInProgress | ManagerInProgress | Calibration | Done
    public Guid CreatedById { get; set; }
}

public class ReviewResponse
{
    public Guid Id { get; set; }
    public Guid ReviewCycleId { get; set; }
    public Guid RevieweeId { get; set; }         // кого оценивают
    public Guid AuthorId { get; set; }           // кто оценивает (==RevieweeId для self)
    public string AuthorType { get; set; }       // "self" | "manager" | "skip-level" (peer — v4)
    public Guid? FormVersionId { get; set; }     // snapshot of form used
    public Guid SubmissionId { get; set; }       // form submission с ответами
    public DateTime SubmittedAt { get; set; }
}

public class ReviewCalibration
{
    public Guid Id { get; set; }
    public Guid ReviewCycleId { get; set; }
    public Guid RevieweeId { get; set; }
    public int FinalPerformanceRating { get; set; }   // 1-5
    public int FinalPotentialRating { get; set; }     // 1-3 (low/medium/high)
    public string BoxLabel { get; set; }               // "Future Star", "Solid Contributor"...
    public string ActionPlan { get; set; }            // markdown: what happens next
    public DateTime CalibratedAt { get; set; }
    public List<Guid> CalibratedByIds { get; set; }    // managers who participated
    public bool SharedWithReviewee { get; set; }       // calibration is confidential by default
}
```

`ReviewCycle` — это quarter/yearly setup, который координирует sequence шагов. `ReviewResponse` — submission, привязанный к cycle. `ReviewCalibration` — финальное решение, принятое группой менеджеров.

## Process: multi-stage ritual как 4 ProcessTemplate-а

Performance Review — это **multi-stage ritual**, не одна форма. Stages:
1. **Self-Review** (per reviewee, deadline +14 days).
2. **Manager Review** (per pair, deadline +21 days, after self).
3. **Calibration** (per team, single meeting, scheduled).
4. **Delivery** (per reviewee, 1-1 with manager).

**Multi-stage реализуется через несколько `ProcessTemplate`-ов** с shared `ReviewCycle`:

| Stage | ProcessTemplate | RitualSchedule | Cadence |
|---|---|---|---|
| Self-Review | `process_template_self_review` | `schedule_1` | `0 0 14 0 0` (через 14 дней после cycle start) |
| Manager Review | `process_template_manager_review` | `schedule_2` | `0 0 21 0 0` (через 21 день) |
| Calibration | `process_template_calibration_prep` | `schedule_3` | `0 0 22 0 0` (через 22 дня, pre-meeting) |
| Delivery 1-1 | `process_template_1on1_delivery` | `schedule_4` | `0 0 30 0 0` (через 30 дней) |

Все 4 шага — это **отдельные ritual schedules**, которые активируются автоматически на разных этапах cycle (с `schedule.active_from` полем). Лид настраивает cycle один раз → система сама разворачивает 4 параллельных ritual-а для каждого члена команды.

## Self-Review форма

```json
{
  "pages": [
    {
      "id": "highlights",
      "title": "Highlights this period",
      "elements": [
        { "id": "wins", "type": "longtext", "label": "What were your most significant wins?",
          "required": true, "minLength": 100 },
        { "id": "okr_summary", "type": "okr_summary", "label": "OKR progress this period",
          "source": "user.assigned_goals", "prefill": true },
        { "id": "skill_wheel_summary", "type": "skill_wheel_summary",
          "label": "Competency shifts since last review",
          "source": "user.computed_gaps", "period": "this_cycle", "prefill": true }
      ]
    },
    {
      "id": "growth",
      "title": "Growth & gaps",
      "elements": [
        { "id": "growth_areas", "type": "longtext", "label": "Where did you grow most?" },
        { "id": "challenges", "type": "longtext", "label": "Where did you struggle?",
          "required": true },
        { "id": "support_needed", "type": "longtext", "label": "What support do you need from your manager?" }
      ]
    },
    {
      "id": "next_period",
      "title": "Goals for next period",
      "elements": [
        { "id": "next_focus", "type": "longtext", "label": "Top 3 priorities for next quarter",
          "required": true },
        { "id": "aspiration", "type": "longtext", "label": "Where do you want to be in 12 months?" }
      ]
    }
  ]
}
```

`okr_summary` и `skill_wheel_summary` — **новые custom field types** (через field-registry), которые автогенерируют prefill из существующих `Goal` и `ComputedGap`. Member не печатает, что уже знает система.

## Manager Review форма

```json
{
  "pages": [
    {
      "id": "self_review_summary",
      "title": "Self-review alignment",
      "elements": [
        { "id": "self_review", "type": "static_text", "source": "linked.self_review.data",
          "label": "What your report wrote", "readonly": true }
      ]
    },
    {
      "id": "manager_assessment",
      "title": "Manager assessment",
      "elements": [
        { "id": "highlights", "type": "longtext", "label": "Significant wins observed" },
        { "id": "challenges", "type": "longtext", "label": "Where did they fall short?" },
        { "id": "skill_wheel_manager", "type": "skill_wheel",
          "competencies_from": "self_review.skill_wheel.competencies",
          "label": "Your assessment of their competencies" },
        { "id": "performance_rating", "type": "rating", "label": "Overall performance", "scale": 5 },
        { "id": "potential_rating", "type": "rating", "label": "Future potential (1-3)", "scale": 3 }
      ]
    },
    {
      "id": "calibration_inputs",
      "title": "For calibration",
      "elements": [
        { "id": "development_priorities", "type": "longtext", "label": "Top development priority" },
        { "id": "next_period_focus", "type": "longtext", "label": "Suggested focus next quarter" }
      ]
    }
  ]
}
```

`potential_rating: 1-3` — это **future potential axis** для 9-box. `performance_rating: 1-5` — current performance. Эти два значения — входные данные для calibration meeting.

`skill_wheel_manager` — копирует competencies из self-review (через `competencies_from`), чтобы manager rate-ит по тому же набору. Это даёт apples-to-apples comparison.

## Calibration meeting (synchronous, 90-120 min)

**Не** submission-driven процесс. Это **отдельный шаг**, который происходит оффлайн (или в zoom), но с structured support.

В системе:
- `ReviewCalibration` entity — финальное решение по каждому reviewee.
- Dashboard виджет "Calibration view" — показывает всех reviewees команды с self/manager ratings, anchor evidence, suggested box.
- Manager предварительно rate-ит async (свой `ReviewResponse` с типом `manager-pre-calibration`).
- На встрече менеджеры обсуждают, корректируют, фиксируют в `ReviewCalibration`.

### Рекомендации для calibration design (из research)

- Каждый manager rate-ит **async** до встречи (с 50-word rationale per person).
- На встрече начинают с **Star box** (самые ясные случаи) и **Underperformer box**.
- Facilitator challenge-ит placements без evidence.
- **Demographic pattern review** — обязателен. Если все "high potential" одного возраста/пола/бэкграунда — это calibration finding, не coincidence.
- **Action plan per person** (development, succession, retention, exit) — обязателен.
- **НЕ** сообщать box-label сотруднику — обсуждать как normal performance conversation.

## 9-box grid (стандарт)

| | Low performance | Meets | Exceeds |
|---|---|---|---|
| **High potential** | Rough Diamond (coach intensively) | Future Star (fast-track, stretch) | (rare) |
| **Medium potential** | Inconsistent Player (clear expectations) | Core Player (stable, role depth) | (rare) |
| **Low potential** | Underperformer (PIP, consider fit) | Solid Contributor (recognition, lateral growth) | (rare) |

В нашей реализации — это **read-only** 3×3 grid, заполненный на основе `ReviewCalibration` для всех members команды.

### Frontend widget

```vue
<template>
  <div class="nine-box">
    <div v-for="row in 3" :key="row" class="row">
      <div v-for="col in 3" :key="col" class="cell"
           :class="boxClass(row, col)">
        <div v-for="person in peopleAt(row, col)" :key="person.id"
             class="person-chip">
          {{ person.name }}
        </div>
      </div>
    </div>
  </div>
</template>
```

`boxClass(row, col)` — комбинация performance (col) × potential (row). Цвет фона: future star (зелёный) / core player (синий) / solid contributor (серый) / etc. Клик на человека → side panel с деталями (manager evidence, self evidence, gap, action plan).

## Delivery 1-1 (закрытие цикла)

Это **обычный 1-1 пресет** из v2, привязанный к `ReviewCycle` через `process_metadata`. Лид берёт `ReviewCalibration` для каждого report-а, проводит 1-1, помечает как `delivered`.

**Важно:** calibration output (`ActionPlan`, `FinalPerformanceRating`) **не** показывается в форме, которую видит reviewee. Manager сам структурирует разговор.

## Action plan и follow-up

Из research: **action plan — обязателен, иначе весь процесс — labeling exercise.**

```csharp
public class ActionPlan
{
    public Guid Id { get; set; }
    public Guid ReviewCalibrationId { get; set; }
    public Guid RevieweeId { get; set; }
    public Guid OwnerId { get; set; }            // обычно manager
    public string Category { get; set; }         // "development" | "succession" | "retention" | "exit" | "recognition"
    public string Description { get; set; }      // markdown
    public DateTime DueDate { get; set; }
    public string Status { get; set; }           // "open" | "in_progress" | "done" | "cancelled"
    public DateTime CompletedAt { get; set; }
}
```

`ActionPlan` — отдельная entity (не JSONB), потому что планы — это **work items**, которые трекаются. Они появляются в:
- Manager 1-1 (лид сам формулирует)
- Reviewer Dashboard (status tracking)
- Quarterly skill wheel review (prefill в next cycle: "what did we commit to last cycle?")

## Privacy и security

### Self-review vs manager review

- **Self-review** видит: только `self` (до `SubmittedAt`), потом и `manager` (после `SubmittedAt` менеджера).
- **Manager review** видит: self + свой собственный draft.
- **Calibration output** видит: только manager + PM (НЕ reviewee).
- **Action plan** после 1-1: виден manager и reviewee (через 1-1 форму).

Это **role-based access control** поверх `ReviewResponse` — `AuthorizationHandler<ReviewResponse>` проверяет claim `user.id` против `authorId` или `revieweeId` + role.

### 9-box confidentiality

Per research: **box-label НЕ сообщается сотруднику**. В нашей системе `ReviewCalibration.SharedWithReviewee = false` по умолчанию. Manager в 1-1 может рассказать verbally, но в UI reviewee видит только `finalPerformanceRating` (1-5) и `actionPlan` (что лид сам ввёл в свободной форме).

### Append-only для review-data

`ReviewResponse` и `ReviewCalibration` immutable, как и `Submission`. Пересчёт → новая запись. Audit-trail через `promptHash`/`submission_id` references.

## Dashboard виджеты

| Виджет | Источник | Что показывает |
|---|---|---|
| **Calibration view** | `ReviewResponse` aggregated | Side-by-side self vs manager per person |
| **9-box grid** | `ReviewCalibration` | 3×3 matrix with people chips |
| **Action plan status** | `ActionPlan` | Open/in-progress/done per reviewee |
| **Cycle timeline** | `ReviewCycle` | Stage progress: SelfInProgress / ManagerInProgress / Calibration / Done |
| **Submission rate** | `ReviewResponse` count / expected | % заполнивших self vs manager review |

## Что НЕ входит в MVP-2

- **360 multi-rater**: v4
- **Self-rating inflation detection (analytics)**: nice-to-have, v3
- **Compensation linking**: out of scope (это HR-системы)
- **Promotions tracking**: out of scope (есть `Goal` cascade, можем link'нуть promotion goal, но не full workflow)
- **Cross-team calibration** (multi-team): single-team только в MVP-2, multi-team в v4
- **Calibration pre-meeting async voting** (каждый manager rate-ит async, потом votes sync): manual сейчас, automated в v3
- **Manager pre-calibration как отдельный submission type**: managers используют тот же `manager_review` форму, но без фиксации

## Открытые вопросы

**Q1. Review cycle: only quarterly, или configurable (half-year, annual)?**
HR-практика: tech компании — quarterly, другие — annual. Рекомендация: **configurable** через `ReviewCycle.PeriodEnd - PeriodStart`, но UX-пресеты = quarterly / half-yearly / annual.

**Q2. 9-box: 3×3 или 4×4?**
3×3 — стандарт, проще объяснить, проверено десятилетиями. Рекомендация: **3×3 в MVP-2**, 4×4 = v4 customization.

**Q3. Multi-team calibration (Director + 2 PMs калибруют 2 команды одновременно)?**
Single-team в MVP-2 (для ясности), multi-team cross-calibration в v4.

**Q4. Кто может создавать `ReviewCycle`?**
PM создаёт для своей команды, Director — для нескольких команд (multi-team). HR может видеть все.

**Q5. Что делать, если reviewee не заполнил self-review до deadline?**
Reminder cascade, escalation to manager. Если в `SelfDeadline + 5 days` пусто — manager может заполнить на основе имеющихся данных + помечать "no self-review submitted". Рекомендация: blocking (manager не может rate-ит, пока self либо не заполнен, либо явно skipped).

## Sequence в MVP-2

| Sprint | Что | Entities |
|---|---|---|
| Sprint 6 (parallel) | ReviewCycle + ReviewResponse + ReviewCalibration | ReviewCycle, ReviewResponse, ReviewCalibration |
| Sprint 6 (parallel) | 4 ProcessTemplate-а для self / manager / calibration prep / delivery | (templates) |
| Sprint 7 | Calibration dashboard виджеты; 9-box UI; action plan workflow | (UI), ActionPlan |
| Sprint 7 (polish) | Performance review delivery 1-1 link; demographic pattern check | (process) |

## Связанные документы

- [architecture.md](../architecture.md) — общая архитектура v2
- [cases/skill-wheel.md](skill-wheel.md) — Skill Wheel case (manager rating в Manager Review = skill_wheel field)
- [cases/okr.md](okr.md) — OKR case (okr_summary в Self-Review = auto-pull из Goal hierarchy)
- [processes.md](../processes.md) — пресеты, включая "1-1 Weekly" (delivery stage)
- [roadmap.md](../roadmap.md) — спринтовое планирование
