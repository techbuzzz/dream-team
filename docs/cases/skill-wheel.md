# Skill Wheel Case

> Глубокий дизайн Skill Wheel в рамках архитектуры v2: как competency assessment, gap analysis и individual development plan реализуются через существующий form engine без интеграций и без отдельных модулей.

## Что это

Skill Wheel — это **не один тип формы**, а **связка из трёх artifact-ов**, которая работает как единый ритуал:

1. **Role Profile** — целевые уровни по компетенциям для конкретной роли.
2. **Self-Assessment** — submission автора на свои компетенции.
3. **Manager Review** (опционально) — submission лида на того же человека.

Gap = `target - observed`. На этом строится Individual Development Plan (IDP). Минимум для defensible gap — self + manager. 360 (peers, direct reports) добавляется в v4 как расширение.

**Критичные best practices (из research):**
- Self-only rating overstates by ~0.5 уровень. Без second rater gap — это opinion, а не data.
- 1-2 цели в IDP, не пять. Development requires sustained attention over weeks.
- 360 rater group avg показывается только при N≥3 raters, иначе merges в overall.
- 5-7 raters per group — sweet spot.

## Доменные entities (новые)

### RoleProfile + CompetencyTarget

```csharp
public class RoleProfile
{
    public Guid Id { get; set; }
    public string Name { get; set; }              // "Senior Backend Engineer"
    public Guid? ParentRoleId { get; set; }       // иерархия ролей (необязательно)
    public int LevelCount { get; set; }            // 3, 4, 5 — сколько уровней на шкале
    public List<CompetencyTarget> Targets { get; set; }
}

public class CompetencyTarget
{
    public Guid Id { get; set; }
    public Guid RoleProfileId { get; set; }
    public string Category { get; set; }           // "Technical depth"
    public string Competency { get; set; }        // "Backend systems"
    public int TargetLevel { get; set; }           // 3 из 5
    public string BehaviorAnchors { get; set; }   // JSON: { "0": "...", "3": "...", "5": "..." }
}
```

`RoleProfile` — это **шаблон роли**, живёт в каталоге компании. Лиды/HR редактируют, пользователи выбирают свою роль в `User.ProfileId`. Один пользователь — одна роль, иерархия ролей — это tree по `ParentRoleId` (Senior IC → IC, Manager → Senior IC, Director → Manager).

### ComputedGap

```csharp
public class ComputedGap
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }              // кого оцениваем
    public Guid ReviewCycleId { get; set; }      // связь с quarterly cycle
    public string CompetencyId { get; set; }     // "td", "pt"...
    public int? TargetLevel { get; set; }        // из RoleProfile
    public int? SelfLevel { get; set; }
    public int? ManagerLevel { get; set; }
    public int? GapTargetSelf { get; set; }      // target - self
    public int? GapTargetManager { get; set; }   // target - manager
    public int? GapSelfManager { get; set; }     // self - manager (blind spot signal)
    public string SelfEvidence { get; set; }
    public string ManagerEvidence { get; set; }
    public DateTime ComputedAt { get; set; }
}
```

Computed в момент, когда оба submission-а (self + manager) получены. Triggered через Hangfire job после `Session.SelfDeadline` + `Session.ManagerDeadline`. Append-only (как и все submissions-derived entities), пересчитывается только при новой итерации цикла.

## Skill Wheel как тип поля

`skill_wheel` — это **уже зафиксированный custom field type** из v2. Здесь мы его уточняем.

**DSL:**
```json
{
  "id": "competencies",
  "type": "skill_wheel",
  "label": "Self-assessment",
  "categories": [
    "Technical depth",
    "Product thinking",
    "Collaboration",
    "Delivery"
  ],
  "competencies": [
    { "id": "td", "label": "Technical depth", "category": "Technical depth" },
    { "id": "pt", "label": "Product thinking", "category": "Product thinking" },
    { "id": "co", "label": "Collaboration", "category": "Collaboration" },
    { "id": "dl", "label": "Delivery", "category": "Delivery" }
  ],
  "scale": 5,
  "evidenceRequired": true,
  "evidenceSchema": { "type": "longtext", "minLength": 20 }
}
```

**Submission data shape (jsonb):**
```json
{
  "competencies": {
    "td": { "level": 3, "evidence": "Led migration of payments service..." },
    "pt": { "level": 2, "evidence": "..." },
    "co": { "level": 4, "evidence": "..." },
    "dl": { "level": 3, "evidence": "..." }
  },
  "overall_self_confidence": 4
}
```

Zod-builder для этого типа:
```typescript
const skillWheelSchema = z.object({
  competencies: z.record(
    z.string(),
    z.object({
      level: z.number().int().min(0).max(5),
      evidence: z.string().min(20)
    })
  )
});
```

Серверная C#-валидация: тот же контракт через NJsonSchema → JsonSchema → Validator. Гарантирует, что клиент не подменит level или обойдёт evidence-валидацию.

## Process flow: Skill Wheel Review (quarterly)

| Шаг | Что происходит | Какие entities задействованы |
|---|---|---|
| 1. PM/Director создаёт `RitualSchedule` | Cadence `0 10 1 */3 *`, audience = members, audienceConfig = all members of teams | `RitualSchedule`, `ProcessTemplate` (preset "Skill Wheel Quarterly") |
| 2. Scheduler генерирует `ProcessInstance` | По одному на каждого члена команды, на 1-е число квартала | `ProcessInstance` × N |
| 3. Self-Assessment reminder | За 7 дней email + in-app "Submit self-assessment" | `Notification` × N |
| 4. Member заполняет форму | `Submission` от `author = user` → snapshot формы vN | `Submission` (self) |
| 5. Manager Review reminder (если включён) | Через 2 дня после self-deadline, lead получает notification | `Notification` × M (lead на каждого report) |
| 6. Lead заполняет manager version | `Submission` от `author = lead` → linked через `ReviewCycle` | `Submission` (manager) |
| 7. Compute gap | Сравнение self vs target, manager vs target, self vs manager | `ComputedGap` (новая entity) |
| 8. Aggregate в team-level signal | Dashboard виджет "Team skill distribution" | Материализованная view |
| 9. Quarterly AI digest | Лид получает "team competency shift over quarter" | `DigestRun` (расширение §4 v2) |

## Dashboard виджеты

| Виджет | Источник | Что показывает |
|---|---|---|
| **Skill distribution** | Все `ComputedGap` для команды | Heatmap: competency × team member, цвет = gap |
| **Self-manager agreement** | `ComputedGap.GapSelfManager` | Scatter plot: где blind spots (self > manager на 1+) |
| **Top 3 gaps across team** | Агрегация по `GapTargetSelf > 0` | Leaderboard: competency → team count needing work |
| **Skill trajectory** | `ComputedGap` за 4 последних квартала | Line chart per competency: avg gap over time |
| **Personal IDP target** | Latest `ComputedGap` для текущего пользователя | Top 1-2 gaps + suggested next action |

Все виджеты — **read-only projections** через EF Core 10 LINQ на `ComputedGap` (или JSONB aggregation на `Submission.data` для тех, у кого ещё нет cycle).

## IDP — что вытекает из ComputedGap

Из research: **1-2 цели в IDP, не пять**. Алгоритм рекомендации IDP-целей на уровне backend:

```csharp
public class IdpRecommendation
{
    public Guid UserId { get; set; }
    public Guid ReviewCycleId { get; set; }
    public List<IdpTarget> Targets { get; set; }   // 1-2 элемента, не больше
}

public class IdpTarget
{
    public string CompetencyId { get; set; }
    public int CurrentLevel { get; set; }
    public int TargetLevel { get; set; }
    public int Priority { get; set; }              // 1 = highest
    public string Rationale { get; set; }         // "Gap of 2, role-criticality: high"
}
```

Алгоритм приоритизации:
1. Gap size (target - current) — больше = выше приоритет.
2. Role criticality (настраивается в `RoleProfile.CompetencyTarget.Critical`).
3. Strategic relevance (через OKR cascade — если competency связана с active goal, вес выше).

**Ограничение: 1-2 цели**, остальные попадают в `watchlist` (видны в dashboard, но не в active IDP).

## Privacy и security

- **Self-assessment** видит: только `self` (до `SubmittedAt`), потом и `manager` (после `SubmittedAt` менеджера).
- **Manager review** видит: self + свой собственный draft.
- **ComputedGap** — append-only, доступен лиду и reviewee.
- **Anonymous peer feedback (360)** — v4, анонимизация per group (N≥3).

Это **role-based access control** поверх `ComputedGap` и `Submission` — `AuthorizationHandler<>` проверяет claim `user.id` против `authorId` или `revieweeId` + role.

## Что НЕ входит (в рамках MVP-2)

- **360° multi-rater**: 5 rater groups, anonymity по N≥3 — это v4. В MVP-2 только self + manager.
- **Анонимизация per-rater**: только manager видим отдельно, peers / direct reports — в v4.
- **Calibration sessions (9-box)**: см. `performance-review.md`.
- **Behavior-anchored calibration at submission time**: вместо этого trust evidence text + manual review.

## Открытые вопросы

**Q1. `RoleProfile` редактируется кем?**
PM? HR? Director? Рекомендация: **HR** создаёт и редактирует role profiles, **PM** выбирает профиль для своего пользователя, **self-service** для update своей роли (с approval).

**Q2. Multi-role: один пользователь — одна роль, или несколько?**
В реальности backend-инженер может также быть tech lead. Рекомендация: **primary role** (для default competency targets) + **secondary roles** (для additional competencies). На MVP-2 — только primary, multi-role в v3.

**Q3. Evidence per competency required?**
Research говорит: да, evidence защищает от self-inflation. Рекомендация: **required, min 20 chars** per competency. UI может показывать character counter и подсказки "конкретный проект за последний квартал".

**Q4. Skill Wheel — ежеквартально или ежегодно?**
Tech компании — quarterly. Enterprise — annual. Рекомендация: **configurable** через `RitualSchedule.Cadence`, но UX-пресет = quarterly.

**Q5. Что показывать, если `RoleProfile` ещё не назначен?**
Fallback на generic IC profile (L1 targets) + warning в dashboard. Рекомендация: **graceful degradation**, не блокировать самооценку.

## Sequence в MVP-2

| Sprint | Что | Entities |
|---|---|---|
| Sprint 6 | RoleProfile + CompetencyTarget; Skill Wheel preset; ComputedGap job | RoleProfile, CompetencyTarget, ComputedGap |
| Sprint 6 (parallel) | 5 dashboard-виджетов | (UI) |
| Sprint 7 (polish) | IDP recommendation job | IdpRecommendation |

## Связанные документы

- [architecture.md](../architecture.md) — общая архитектура v2
- [cases/okr.md](okr.md) — OKR case (shared `ReviewCycle` entity)
- [cases/performance-review.md](performance-review.md) — Performance Review case
- [processes.md](../processes.md) — пресеты, включая "Skill Wheel Review" template
