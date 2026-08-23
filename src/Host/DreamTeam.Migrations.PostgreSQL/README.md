# DreamTeam.Migrations.PostgreSQL

Central EF Core migrations for DreamTeam. One folder per kept DbContext:

| Folder | DbContext | Notes |
|---|---|---|
| `Identity/` | `DreamTeam.Modules.Identity.Data.IdentityDbContext` | Kept |
| `MultiTenancy/` | `DreamTeam.Modules.Multitenancy.Data.TenantDbContext` | Kept (dormant until v4) |
| `Files/` | `DreamTeam.Modules.Files.Data.FilesDbContext` | Kept |
| `Eventing/` | `DreamTeam.Framework.Eventing.Persistence.EventingDbContext` | Kept (Outbox/Inbox) |

## Migration history status

The migration files in this project are **inherited from the FSH scaffold**. The folder
structure and `*ModelSnapshot.cs` files are intact and the schema they create is correct for
the kept modules. Migration filenames still carry FSH-era timestamps (e.g.
`20251222232937_Initial.cs`) and some include FSH-specific work (e.g. Outbox columns
that may no longer be needed).

**Action deferred to a follow-up workstream:** regenerate the migration history as a single
`00000000000001_DreamTeam_InitialSchema` per DbContext using `dotnet ef migrations add`.
This requires:

```bash
dotnet tool install -g dotnet-ef
# Per DbContext (run from src/Host/DreamTeam.Migrations.PostgreSQL/):
dotnet ef migrations add DreamTeam_InitialSchema --context IdentityDbContext --output-dir Identity
dotnet ef migrations add DreamTeam_InitialSchema --context TenantDbContext    --output-dir MultiTenancy
dotnet ef migrations add DreamTeam_InitialSchema --context FilesDbContext      --output-dir Files
dotnet ef migrations add DreamTeam_InitialSchema --context EventingDbContext   --output-dir Eventing
```

Then delete the inherited migration files. This is mechanical work; the FDS doesn't require
it for MVP-1 (no production data exists to migrate).

## What was removed (Phase 3 of the FSH-strip)

These migration folders are gone (their owning modules were deleted):

`Audit/`, `Billing/`, `Catalog/`, `Chat/`, `Notifications/`, `Tickets/`, `Webhooks/`

Any database that was running the FSH schema and applied those migrations is in an
inconsistent state vs. the kept modules. The recommended migration path is:

1. Take a fresh Postgres for MVP-1.
2. Run `dotnet run --project src/Host/DreamTeam.DbMigrator -- apply` against it.
3. The current migration history will create the kept-module tables.

If a production database with the FSH schema exists, the migration history will need
manual surgery. This is a separate workstream.
