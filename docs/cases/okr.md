# OKR Case

> Глубокий дизайн OKR в рамках архитектуры v2: goals + key results + weekly check-ins + pace computation + linked-field aggregation. Без интеграций, через существующий form engine.

## Что это

OKR в нашей модели — это **3 entity + multi-stage ritual + linked-field killer feature**.

**Ключевые принципы (из research):**
- **Confidence (1-10) и Progress — разные сигналы.** Progress говорит где ты сейчас, Confidence — веришь ли в путь. Confidence может падать при росте progress (ранние вины маскируют риск).
- **Pace computed, not flat threshold.** `expectedPace(t) = start + (target-start) * (t-1)/(T-1)`. Status from math, не "feels worried".
- **3 states, not 10.** On-track / Behind / At-risk. Action attached к каждому, не просто status.
- **Bottom-up alignment, не top-down cascade.** Teams сами определяют свои KRs и link-up через `ParentKeyResultId`. Cascade (top-down) даёт rigidity и bottleneck на верхнем уровне.
- **Each confidence score needs a note** — что изменилось, что блокирует, что будет сделано на следующей неделе.

## Доменные entities (новые)

```csharp
public class Goal
{
    public Guid Id { get; set; }
    public Guid? ParentGoalId { get; set; }     // иерархия: company → dept → team
    public Guid? ParentKeyResultId { get; set; } // опционально: cascade под конкретный KR
    public string Title { get; set; }
    public string Description { get; set; }
    public GoalLevel Level { get; set; }         // Company | Department | Team | Individual
    public Guid OwnerId { get; set; }
    public Guid TeamId { get; set; }
    public GoalStatus Status { get; set; }       // Draft | Active | AtRisk | Achieved | Missed
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class KeyResult
{
    public Guid Id { get; set; }
    public Guid GoalId { get; set; }
    public string Title { get; set; }            // "Reduce churn from 8% to 4%"
    public string Unit { get; set; }             // "percent", "count", "USD"
    public double StartValue { get; set; }
    public double TargetValue { get; set; }
    public double CurrentValue { get; set; }     // пересчитывается из CheckIn-ов или linked_field
    public double? ExpectedPace { get; set; }    // computed
    public string SourceType { get; set; }       // "manual" | "linked_field"
    public Guid? SourceFieldId { get; set; }      // если linked: тянем из submission.data
    public Guid OwnerId { get; set; }
}

public class CheckIn
{
    public Guid Id { get; set; }
    public Guid KeyResultId { get; set; }
    public DateTime WeekStart { get; set; }       // начало недели (ISO week)
    public double? CurrentValue { get; set; }     // optional override
    public int Confidence { get; set; }          // 1-10, required
    public string Note { get; set; }              // required, min 20 chars
    public string Blockers { get; set; }         // explicit field, max 280 chars
    public string CommittedAction { get; set; }   // required: "By next Mon: ship Z"
    public DateTime SubmittedAt { get; set; }
    public Guid AuthorId { get; set; }
}
```

**Структура:** Goal = parent node, KeyResult = measurable child, CheckIn = weekly snapshot. `ParentGoalId` и `ParentKeyResultId` дают два режима cascade: под Objective или под конкретный KR. **Современная best practice — bottom-up alignment, не top-down cascade**: teams сами определяют свои KRs и link-up.

## Status computation (Pace, не flat threshold)

**Confidence (1-10):** вводится человеком, простая шкала.
**Pace:** `expectedPace(t) = start + (target - start) * (t - periodStart) / (periodEnd - periodStart)`. Сравниваем с `currentValue`. Status:

- **on-track**: `currentValue >= expectedPace * 0.9`
- **behind**: `expectedPace * 0.7 <= currentValue < expectedPace * 0.9`
- **at-risk**: `currentValue < expectedPace * 0.7` ИЛИ `confidence <= 3` два раза подряд
- **achieved**: `currentValue >= target`

Это даёт объективный status, не "feels worried". **Confidence** остаётся separate signal — может падать при росте progress.

### Алгоритм на C#

```csharp
public static class KrStatusCalculator
{
    public static KrStatus Compute(KeyResult kr, double? latestValue = null)
    {
        if (latestValue is null) latestValue = kr.CurrentValue;

        if (latestValue >= kr.TargetValue)
            return KrStatus.Achieved;

        if (!kr.ExpectedPace.HasValue || kr.PeriodEnd <= DateTime.UtcNow)
            return ComputeFinalMissed(kr, latestValue.Value);

        var ratio = (latestValue.Value - kr.StartValue) / (kr.TargetValue - kr.StartValue);
        var expectedRatio = (DateTime.UtcNow - kr.PeriodStart).TotalDays
                          / (kr.PeriodEnd - kr.PeriodStart).TotalDays;

        if (ratio >= expectedRatio * 0.9) return KrStatus.OnTrack;
        if (ratio >= expectedRatio * 0.7) return KrStatus.Behind;
        return KrStatus.AtRisk;
    }
}
```

## Linked Key Results: killer feature без интеграций

`KeyResult.SourceType = "linked_field"` — это killer-фича MVP-2. Поле `SourceFieldId` указывает на field в `Submission.data` другой формы.

```csharp
public class LinkedFieldMapping
{
    public Guid KeyResultId { get; set; }
    public string Aggregation { get; set; }      // "sum" | "avg" | "count" | "latest"
    public string AggregationField { get; set; } // "count", "score", "value"
    public string FilterJson { get; set; }       // jsonb: "data.status == 'done'"
}
```

Hangfire job пересчитывает `KeyResult.CurrentValue` раз в час: aggregate `Submission.data` где `FormVersion.field.id == SourceFieldId`, применить filter, apply aggregation, записать.

### Примеры

- KR "ship 10 features this quarter" — линкуется на `count(features)` в `Submission.data` пресета "Sprint Review".
- KR "achieve 95% test coverage" — линкуется на `coverage` в submission от "Sprint Health".
- KR "onboard 5 new customers" — линкуется на `count(customers)` в submission "Customer Pulse".
- KR "team energy >= 3.5 avg" — линкуется на `avg(energy)` в submission "Daily Standup".

Это **закрывает integration-эффект без интеграций** — KR live-update из существующих submissions. Если у вас уже есть "Sprint Review" preset и лиды в нём отмечают "shipped features", эти данные автоматически кормят OKR.

## Process flow: weekly OKR check-in

| Шаг | Что | Когда |
|---|---|---|
| 1 | `RitualSchedule` для OKR Check-in | Cadence `0 9 * * 1` (каждый понедельник 09:00) |
| 2 | Scheduler создаёт `ProcessInstance` | За неделю до check-in |
| 3 | Reminder | За 30 мин |
| 4 | Owner открывает форму | Форма: matrix `kr_progress` с автозаполнением из linked fields, плюс `confidence`, `note`, `blockers`, `committed_action` per KR |
| 5 | Owner заполняет | `Submission.data = { kr_progress: [{ krId, current, confidence, note, blockers, action }], week: "2026-W34" }` |
| 6 | Server-side aggregation | Из `Submission.data` создаются `CheckIn`-ы, пересчитываются `KeyResult.CurrentValue` и `Status` |
| 7 | Dashboard update | "OKR at-risk count" виджет пересчитывается |
| 8 | Friday digest (опционально) | Если cron = Friday: weekly summary "what's at risk" |

### Форма OKR Check-in (template)

```json
{
  "pages": [
    {
      "id": "kr_update",
      "title": "Per-KR update",
      "elements": [
        {
          "id": "kr_progress",
          "type": "matrix",
          "label": "Update per key result",
          "rowsSource": "team.active_goals.key_results",
          "columns": [
            { "id": "current", "label": "Current", "type": "number", "prefill": "linkedFieldValue" },
            { "id": "status", "label": "Status", "type": "select",
              "options": ["on-track", "behind", "at-risk"],
              "prefill": "computedFromPace" },
            { "id": "confidence", "label": "Confidence 1-10", "type": "rating", "scale": 10, "required": true },
            { "id": "moved", "label": "What moved it?", "type": "longtext", "required": true }
          ]
        }
      ]
    },
    {
      "id": "blockers",
      "title": "Blockers & next week",
      "elements": [
        { "id": "blockers", "type": "longtext", "label": "What's blocking?" },
        { "id": "next_week", "type": "longtext", "label": "Specific action you commit to before next check-in" }
      ]
    }
  ]
}
```

`prefill: "linkedFieldValue"` и `prefill: "computedFromPace"` — это **новая фича DSL-типов**: поля могут быть pre-populated из подключённых источников. Renderer показывает значение, user может override (если знает точнее).

## Mid-quarter alignment review (week 6)

**Через 6 недель квартала** — отдельный ritual (ежеквартальный, разовый). PM/Director получает отчёт:
- Tree visualization: `Goal → KR` с цветом по status
- Gaps: KR, у которых нет owner
- Coverage: ветки без KR (organisation/team не закрывает)
- Cross-team dependencies: KRs из разных команд, ссылающиеся на один parent KR

Это **не** AI digest — это **pure query** через `Goal` tree с цветовой разметкой. Делается за 1 SQL-запрос + D3-tree-renderer на фронте. Заменяет 90% функциональности 9-box, но в координатах OKR.

## Dashboard виджеты

| Виджет | Источник | Что показывает |
|---|---|---|
| **OKR tree (стратегическая карта)** | `Goal` hierarchy | Tree с цветом по status, drilldown до KR |
| **At-risk count** | `KeyResult` где status=AtRisk | Counter, click → список |
| **Confidence trend** | `CheckIn` за 4 недели | Sparkline per KR |
| **Pace vs progress scatter** | `KeyResult.ExpectedPace` vs `CurrentValue` | Scatter: где progress норм, но pace отстаёт |
| **Linked-field freshness** | `Submission` count feeding each linked KR | "Last data point: 3 days ago" indicator |

## Privacy

- **CheckIn** видит: owner, его lead, и admin/Director.
- **KeyResult.CurrentValue** — derived state, доступен всем в команде (для alignment).
- **LinkedFieldMapping** агрегаты только (`sum`, `avg`, `count`, `latest`), **не raw values** — privacy contract на уровне enum.
- **Goal cascade** виден всем в команде (alignment требует visibility).

## Server-side aggregation job

```csharp
public class LinkedKrAggregationJob
{
    public async Task RecalculateAllActiveKrs(CancellationToken ct)
    {
        var activeKrs = await db.KeyResults
            .Include(kr => kr.Goal)
            .Where(kr => kr.SourceType == "linked_field" 
                       && kr.Goal.Status == GoalStatus.Active)
            .ToListAsync(ct);

        foreach (var kr in activeKrs)
        {
            var mapping = await db.LinkedFieldMappings
                .FirstAsync(m => m.KeyResultId == kr.Id, ct);

            var submissions = await db.Submissions
                .Where(s => s.ProcessInstance.ScheduleId != null
                         && s.Data.ContainsKey(mapping.AggregationField))
                .Where(/* apply FilterJson */)
                .ToListAsync(ct);

            var newValue = mapping.Aggregation switch
            {
                "sum"   => submissions.Sum(s => Convert.ToDouble(s.Data[mapping.AggregationField])),
                "avg"   => submissions.Average(s => Convert.ToDouble(s.Data[mapping.AggregationField])),
                "count" => submissions.Count,
                "latest"=> submissions.OrderByDescending(s => s.SubmittedAt)
                                     .First().Data[mapping.AggregationField] is var v 
                                     ? Convert.ToDouble(v) : 0,
                _ => 0
            };

            kr.CurrentValue = newValue;
            kr.Status = KrStatusCalculator.Compute(kr);
        }
        await db.SaveChangesAsync(ct);
    }
}
```

Запускается через Hangfire recurring job каждый час.

## Что НЕ входит в MVP-2

- **AI-based check-in summaries**: digest только на уровне team, не per-KR. Per-KR AI digests — v4.
- **Cross-team dependency graph visualization**: только mid-quarter review выгружает, постоянного view нет.
- **KR re-writing mid-quarter** (изменение target): UI есть (CRUD), но нет workflow approval.
- **OKR grading** (0.0-1.0 score): есть в research, но добавлено как v3 feature.
- **Historical OKR comparison** (YoY, QoQ): только basic trajectory в v3.

## Открытые вопросы

**Q1. `CheckIn.Note` обязателен?**
Research говорит: yes, every confidence score should have a note. Рекомендация: **required, min 20 chars**. Это дисциплинирует.

**Q2. OKR cascade: top-down или bottom-up?**
Research: **bottom-up alignment** побеждает top-down cascade в реальности. Рекомендация: реализуем оба, но **UI по умолчанию** = bottom-up (лид создаёт свои KR и link-up).

**Q3. Кто может создавать Goals?**
В типичной practice: company OKRs — leadership, team OKRs — PM/lead, individual OKRs — member+lead. Рекомендация: создание через `GoalLevel` (Company → только Director+PM, Team → PM+lead, Individual → member+lead).

**Q4. Что показывать, если нет активных OKR?**
Weekly check-in с пустой matrix — UI должен gracefully показывать "No active OKRs" + CTA "Create goals for this quarter". Рекомендация: CTA ведёт на Goal management page, не блокировать check-in.

**Q5. LinkedFieldMapping: cross-team aggregation?**
`AggregationFilter` может включать `team_id == this`. По умолчанию — фильтр на team, в которой находится KR. Рекомендация: в MVP-2 только same-team, cross-team aggregation — v3.

## Sequence в MVP-2

| Sprint | Что | Entities |
|---|---|---|
| Sprint 4 | Goal + KeyResult + CheckIn (OKR core) | Goal, KeyResult, CheckIn |
| Sprint 5 | LinkedFieldMapping + aggregation job; OKR preset | LinkedFieldMapping |
| Sprint 5 (parallel) | Mid-quarter alignment review query | (UI) |
| Sprint 6 | 5 dashboard-виджетов | (UI) |
| Sprint 7 (polish) | prefill extensions в Zod-builder; rate status banner | (renderer) |

## Связанные документы

- [architecture.md](../architecture.md) — общая архитектура v2
- [cases/skill-wheel.md](skill-wheel.md) — Skill Wheel case (shared `ReviewCycle` entity, `ComputedGap` для IDP)
- [cases/performance-review.md](performance-review.md) — Performance Review case (review включает OKR summary)
- [processes.md](../processes.md) — пресеты, включая "OKR Check-in" template
