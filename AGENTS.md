# DreamTeam

> **Form-first rituals platform for Tech Leads / Team Leads / Squad Leads.** Build a form once; the system sends it to the team on a schedule, aggregates responses, draws a dashboard, and produces a weekly digest via a local LLM.

This file is the canonical guide for **all** AI coding tools (Claude Code, Gemini CLI, Cursor, Codex, …). `CLAUDE.md` and `GEMINI.md` are thin bridges that import this file — edit conventions **here**, not there.

Detailed conventions live in `.agents/rules/` and `docs/` — **read the relevant file before working in that area** (see the index below). Keep this file lean.

## Current state of the repo

This repo **is** the DreamTeam project, bootstrapped from the **FullStackHero .NET Starter Kit** (FSH) as the technical foundation. The FSH scaffold has been rebranded and stripped: namespaces are `DreamTeam.*` everywhere, the FSH-only modules (Auditing, Billing, Catalog, Chat, Tickets, Webhooks, FSH Notifications) are deleted, and the dead template machinery (`src/Tools/CLI/`, `templates/`, `.template.config/`, `deploy/terraform/`) is gone. **The repo is now ready to receive the first feature workstream — the `Forms` module per MVP-1 (E1.1–E1.6 in the FDS roadmap).**

| Layer | What is there today | What is coming next |
|---|---|---|
| `src/Host/DreamTeam.Api` + `DreamTeam.AppHost` + `DreamTeam.DbMigrator` + `DreamTeam.Migrations.PostgreSQL` | The renamed .NET 10 host projects. Buildable, tests passing (650 of 654 unit tests; 4 pre-existing FSH-era architecture-rule failures documented). | The `Forms` module's endpoints land here. |
| `src/BuildingBlocks/*` (12 projects) | `DreamTeam.Framework.*` namespaces. BuildingBlocks stay generic; lightly extended with `ISecurityAudit` / `IAuditClient` no-op contracts in `Framework.Shared.Identity` (replaces the deleted FSH Auditing module; real impl lands in MVP-2+). | No planned changes. |
| `src/Modules/Identity` (kept) | FSH identity module with `DreamTeam.*` namespace; serves as DreamTeam's auth foundation. Roles alignment to `TeamLead/PM/DeliveryManager/Member` (per FDS) is deferred. | The Forms module's first user-facing permission check. |
| `src/Modules/Multitenancy` (kept, dormant) | FSH multitenancy module; middleware is OFF in `Program.cs` (single-tenant for MVP-1); billing integration stripped from `CreateTenant`/`RenewTenant` handlers (default 12-month validity, no plan key). | v4 per FDS — re-enable middleware + integrate billing. |
| `src/Modules/Files` (kept) | FSH Files module (MinIO attachments). Works as-is. | No planned changes. |
| `infra/` (new) | Docker Compose stack mirroring the FDS service set: postgres, valkey, minio, mailhog, optional ollama. Makefile targets. | Add the API + web containers (currently in `--profile full`). |
| `clients/admin` + `clients/dashboard` (kept) | FSH React 19 + Vite + TS SPAs. The only working UI today. | The future Nuxt 4 `apps/web` replaces them. |
| `docs/` (untouched) | DreamTeam design docs (architecture, roadmap, processes, cases). The source of truth. | — |
| `apps/`, `packages/` (not yet) | **Not present** — per the top-level README's documented target. | Apps/web (Nuxt 4) lands as a separate workstream post-MVP-1. |
| `.template.config/`, `templates/`, `src/Tools/CLI/`, `deploy/terraform/`, `deploy/dokploy/` | **Deleted** — FSH template machinery, not needed for a single-product repo. | — |

The FSH-strip prep (Phase 0-6) is in this repo's git history. The first commit after the strip is the prep's net diff. New work builds on the post-strip state.

## What DreamTeam is

**Army knife for the Tech Lead.** 1-1, daily, retro, OKR check-in, skill wheel review are not separate features — they are **preset forms** that a lead copies and configures in minutes. The system handles scheduling, audience, notifications, aggregation, and weekly AI digest.

- **Positioning** — Not a Slack bot (Standuply/Geekbot), not a perf tool (Lattice/15Five), not a form builder (Typeform). Web app, self-hostable, form-first, AI digest built in.
- **Killer feature** — **AI Digest** via self-hosted LLM (Ollama / vLLM). Data never leaves the customer's DB.
- **License** — TBD (likely AGPLv3 + commercial dual-license).

Full positioning, architecture, and roadmap live in the docs — see **Rules index** below.

## Tech stack (target)

| Backend | Frontend | Async & data |
|---|---|---|
| .NET 10 (LTS to 14.11.2028) | Nuxt 4 (stable since 16.07.2025; Nuxt 3 EOL 31.07.2026) + Vue 3 | Hangfire (Postgres storage) — scheduler + jobs |
| Minimal APIs + EF Core 10 | VeeValidate + Zod + shadcn-vue (Reka UI) | PostgreSQL 16 + JSONB (EF Core 10 Complex Types) |
| ASP.NET Identity + JWT + refresh rotation | Pinia state | MinIO (S3-compatible) — attachments |
| FluentValidation | | Ollama → vLLM — digest LLM |
| Cronos (cron parsing) | | MailKit + FluentEmail + MailHog — email |

> The current `src/` is .NET 10 / EF Core 10 (matches). The current `clients/*` is React 19 + Vite (does **not** match the target Nuxt 4 — see the FSH notes in "Scaffolding from FSH" below).

## Build & run (what actually works today)

The renamed .NET host + the kept React clients run. Use these commands:

```bash
# Whole stack (Postgres + pgAdmin + Redis + MinIO + migrator + API + both React apps)
dotnet run --project src/Host/DreamTeam.AppHost   # one-time: npm install in clients/admin & clients/dashboard

dotnet build src/DreamTeam.slnx                   # build backend
dotnet run --project src/Host/DreamTeam.Api       # API only → https://localhost:7030 (/scalar)
dotnet test  src/DreamTeam.slnx                   # tests — integration tests REQUIRE Docker

cd clients/admin     && npm install && npm run dev   # → http://localhost:5173
cd clients/dashboard && npm install && npm run dev   # → http://localhost:5174
```

Migrations / seed (DreamTeam DbMigrator, separate step):
```bash
dotnet run --project src/Host/DreamTeam.DbMigrator -- apply
# seed-demo is currently a no-op (FSH demo data removed); DreamTeam seed lands later.
```

Docker-only path (no Aspire):
```bash
make -C infra up            # postgres, valkey, minio, mailhog
dotnet run --project src/Host/DreamTeam.Api
```

**Ports:** API 7030 (https)/5030 (http) · admin 5173 · dashboard 5174 · Postgres 5432 · pgAdmin 5050 · Valkey 6379 · MinIO 9000/9001 · MailHog 1025/8025.

When the Nuxt 4 `apps/web` lands, the React-client dev commands will be replaced by `apps/web`-based ones. **Update this section when that happens.**

## Branching & PRs

Single long-lived branch: **`main`** (default). Branch from and target `main`; stable releases are cut from `v*` tags. CI is split into path-scoped **Backend CI** (`src/**`, `apps/api/**`) and **Frontend CI** (`clients/**`, `apps/web/**`) workflows. Use Conventional Commits — match the existing history (`feat(rituals): ...`, `fix(digest): ...`).

## Golden rules (do not break)

1. **Form is data, not code** — a form is JSON in `FormVersion.Schema`; the renderer is generic. Never build per-ritual UI.
2. **Snapshot-on-publish** — every `ProcessInstance` points at a specific `FormVersion`. Renaming or editing a template never mutates past instances.
3. **Append-only submissions** — corrections are new rows. `Submission` is immutable; an `isCompensating` flag points at the prior submission it amends.
4. **Schedules are first-class** — `RitualSchedule` is separate from `ProcessTemplate` (cadence / audience can change without breaking past instances).
5. **Notifications are intents, not sends** — `Notification` (intent) and `NotificationDelivery` (attempt with channel + status + retry) are separate.
6. **AI is pluggable, data is sovereign** — digest goes through `IDigestLlm`; for MVP-2 the default is self-hosted Ollama. Customer data must not leave their infrastructure.
7. **Postgres JSONB over EAV** — `FormVersion.Schema` and `Submission.Data` are `jsonb` with GIN (`jsonb_path_ops`) indexes. Don't add an EAV layer unless analytics queries actually need it.
8. **Tenant isolation is default-ON** for any per-tenant data (when multi-tenant lands in v4) — opt out only via `IGlobalEntity`.
9. **Mediator handlers must be `public sealed`**, return `ValueTask<T>`, and `.ConfigureAwait(false)` every await (carried over from FSH base).
10. **Docs + changelog travel with the change** — a user-facing change (feature, endpoint, preset, infra) isn't done until `docs/` is updated to match.

## Rules index — read the relevant file before you work

**DreamTeam design (source of truth — read first when working on features)**

| Working on… | Read |
|---|---|
| The big picture — form engine, rituals, notifications, dashboard, digest | `docs/architecture.md` |
| v1 form-engine design (historical, still relevant — submission immutability, DSL, builder) | `docs/architecture-v1.md` |
| Sprint plan, MVP-cut, metrics, exit criteria | `docs/roadmap.md` |
| Preset catalog (1-1, Daily, Retro, Skill Wheel, OKR) — what fields, what cadence, what audience | `docs/processes.md` |
| Deep cases — Skill Wheel, OKR, Performance Review (cross-case flows, extra entities, widget list) | `docs/cases/README.md` and the three `docs/cases/*.md` files |
| Admin → dashboard design unification (a parallel program) | `docs/superpowers/specs/2026-05-28-admin-dashboard-design-unification-design.md` |

**FSH scaffolding rules (apply to current FSH code in `src/` and `clients/`; will be replaced as DreamTeam modules land)**

| Working on… | Read |
|---|---|
| Module structure, boundaries, registration, DI, middleware order, config | `.agents/rules/architecture.md` |
| Endpoints, CQRS, validation, exceptions, permissions, versioning | `.agents/rules/api-conventions.md` |
| EF Core, entities, migrations, tenant isolation, query filters | `.agents/rules/database.md` |
| Cross-module events, Outbox/Inbox, idempotent handlers | `.agents/rules/eventing.md` |
| Caching (HybridCache/Redis), keys, invalidation | `.agents/rules/caching.md` |
| Background jobs (Hangfire), recurring jobs | `.agents/rules/jobs.md` |
| Outbound HTTP resilience (Polly) | `.agents/rules/resilience.md` |
| Files/blobs, presigned uploads, providers | `.agents/rules/storage.md` |
| CORS, security headers, rate limiting, idempotency, quotas | `.agents/rules/security.md` |
| SignalR / SSE backend | `.agents/rules/realtime.md` |
| Logging, correlation, OpenTelemetry | `.agents/rules/logging.md` |
| Unit test conventions, NetArchTest | `.agents/rules/testing.md` |
| Integration tests (Testcontainers harness + gotchas) | `.agents/rules/integration-testing.md` |
| **Modifying `src/BuildingBlocks`** (read first — it's protected FSH) | `.agents/rules/buildingblocks-protection.md` |
| A specific FSH module's quirks | `.agents/rules/modules/{module}.md` |
| React (admin / dashboard) — FSH patterns | `.agents/rules/frontend/{shared,admin,dashboard}.md` |

**AI tooling resources**

- **Skills** — `.agents/skills/*/SKILL.md`: task recipes. Scaffolders: `add-feature`, `add-entity`, `add-module`, `add-react-page`, `add-full-slice`. Ops: `create-migration`, `add-integration-event`, `add-permission`. Reference: `query-patterns`, `testing-guide`, `mediator-reference`.
- **Workflows** — `.agents/workflows/*.md`: task playbooks (`code-reviewer`, `feature-scaffolder`, `module-creator`, `architecture-guard`, `migration-helper`).

## Coding style

- **Backend (target, .NET 10):** file-scoped namespaces · 4-space indent · explicit types (`var` only when RHS-obvious) · `is null` / `is not null` · pattern matching + switch expressions · `ArgumentNullException.ThrowIfNull` guards · records for DTOs/events/value objects · `default!` for required non-nullable strings. Build runs with `TreatWarningsAsErrors` — warnings fail the build.
- **Frontend (target, Nuxt 4 / Vue 3):** `<script setup lang="ts">` · TypeScript strict · Pinia for state · `definePage` + file-based routing · VeeValidate + Zod for forms · shadcn-vue on Reka UI for components.

## Scaffolding from FSH — what to keep, what to replace

The FSH code in `src/` and `clients/` is **scaffolding** that the prep converted into a DreamTeam-named baseline. As DreamTeam lands:

- **Kept (extend, don't fight):** the modular monolith pattern, BuildingBlocks, Hangfire, Postgres + EF Core, Mediator (source-gen CQRS), FluentValidation, MinIO, identity, observability, multitenancy (dormant for v4).
- **Replace (eventually):** `clients/admin` and `clients/dashboard` (React 19) → `apps/web` (Nuxt 4). The Nuxt 4 migration is a separate workstream that lands post-MVP-1.
- **Add (new modules under `src/Modules/`):** `Forms` (MVP-1), `Rituals` / `Notifications` / `Dashboard` / `Digest` (MVP-2), case-specific (`SkillWheel`, `OKR`, `PerformanceReview`) after MVP-2.
- **Already removed (Phase 3 of the prep):** Auditing (replaced by no-op `ISecurityAudit`/`IAuditClient` contracts in `Framework.Shared.Identity`), Billing, Catalog, Chat, Tickets, Webhooks, FSH Notifications. Don't re-introduce these — DreamTeam doesn't need them.

## Adding things (quick pointers)

- **New module (e.g. `Forms` for MVP-1)** — `src/Modules/Forms/` with `Modules.Forms.Contracts` + `Modules.Forms`. Use the `add-module` skill for the boilerplate. Wire into `Program.cs` Mediator markers + `moduleAssemblies` + `DreamTeam.Api.csproj` ProjectReferences + `DreamTeam.Migrations.PostgreSQL.csproj` (if it has migrations) + `DreamTeam.slnx` + `Architecture.Tests` module list. The "FSH golden rule of FOUR places" still applies: `DreamTeam.Api/Program.cs` Mediator + moduleAssemblies, `DreamTeam.DbMigrator/Program.cs` equivalents, the slnx.
- **New preset (1-1, Daily, Retro, Skill Wheel, OKR)** — define the FormVersion JSON per `docs/processes.md`, register a ProcessTemplate seed, add cadence + audience defaults. Renderer/Builder already generic.
- **New field type (e.g. `skill_wheel`, `matrix-with-source`, `rank`)** — register Vue component + Zod schema in the field-registry. Server validator mirrors the Zod schema. No engine fork.
- **New dashboard widget** — read-only projection over `Submission` / `ProcessInstance` via JSONB aggregation. See widget inventory in `docs/architecture.md` and `docs/cases/README.md`.
- **New notification type** — entry in the `NotificationTemplate` registry (DB-backed, not in code) + retry-policy config. Templates live in DB so лиды can customize phrasing without a deploy.
- **New AI digest section** — extend the system prompt in `docs/architecture.md` and the validation step (no hallucinated user-ids, no invented ticket numbers).

## Next module to add: Forms (MVP-1, E1.1–E1.6)

Per the FDS roadmap, the next module to land is **`DreamTeam.Modules.Forms`**. It implements the form engine (FormVersion, ProcessInstance, Submission, Answer entities) per `docs/architecture-v1.md` §1–§4. The acceptance criteria are in `docs/roadmap.md` under "MVP-1: Form Engine Foundation". Use the `add-module` skill to scaffold; use the `add-feature` skill for each vertical slice (process template CRUD, form version publish, submission write, renderer endpoints).

## Roadmap status

See `docs/roadmap.md` for the full plan. Headline:

- **MVP-1 (4-6 wk)** — Form engine + 1-1 preset + auth + builder/renderer. **Sprint 1-2 of the project; not yet started in code.**
- **MVP-2 (6-8 wk on top)** — Ritual Scheduler + Notification Pipeline + Team Dashboard + AI Digest.
- **MVP-3 (4 wk)** — Polish: visual conditional logic editor, computed fields, calendar integration, PWA, multi-lang digest.
- **v4 (3+ mo)** — Enterprise: SignalR cluster, OpenIddict, multi-tenant, Slack/Teams, vLLM.

Until MVP-1 lands, every change should be justifiable against the MVP-1 exit criteria. Don't build v4 features early.
