# DreamTeam infra

Local development infra for the DreamTeam stack. The current `docker-compose.yml`
brings up everything the API needs: Postgres, Valkey (Redis-compatible), MinIO, and
MailHog. The .NET API itself can run either in a container (`make up-full`) or on the
host (`dotnet run --project src/Host/DreamTeam.Api` — recommended for dev iteration).

## Relationship to the .NET Aspire AppHost

`src/Host/DreamTeam.AppHost` is the **one-command dev path** using .NET Aspire 13.4.0.
It spins up the same services plus the API, with an Aspire dashboard for logs/traces.
**Use the Aspire AppHost if you have the .NET 10 SDK + Aspire tooling installed.**

This `infra/` directory is the **Docker-only path**:
- No Aspire tooling required.
- Closer to production shape (no Aspire sidecars).
- Easy to script from CI.
- Mirror of the FDS's documented service set.

```bash
# One-command dev with Aspire (recommended):
dotnet run --project src/Host/DreamTeam.AppHost

# Or with Docker only:
make up           # postgres, valkey, minio, mailhog
cd src/Host/DreamTeam.Api && dotnet run
```

## Service set

| Service | Image | Port(s) | Purpose |
|---|---|---|---|
| `postgres` | `postgres:16-alpine` | 5432 | Primary store. EF Core 10 + JSONB. |
| `valkey` | `valkey/valkey:9.1.0-alpine` | 6379 | Cache + SignalR backplane (v4) + Hangfire. |
| `minio` | `minio/minio:latest` | 9000/9001 | S3-compatible attachments (presigned PUTs). |
| `minio-init` | `minio/mc:latest` | — | One-shot bucket creation on first start. |
| `mailhog` | `mailhog/mailhog:latest` | 1025/8025 | Dev email catcher. Prod: real SMTP. |
| `api` *(opt-in)* | `dreamteam-api:dev` | 7030 | The .NET API. Built locally. |
| `web-stub` *(opt-in)* | `nginx:alpine` | 3000 | Placeholder for future Nuxt 4 `apps/web`. |
| `ollama` *(opt-in)* | `ollama/ollama:latest` | 11434 | AI digest LLM (MVP-2). Pulls `llama3.1:8b-instruct`. |

## Environment variables

The `api` service receives these env vars (matching `appsettings.Development.json` defaults
in the FSH scaffold). For production, override at the orchestrator / secrets manager.

| Variable | Value | Notes |
|---|---|---|
| `DatabaseOptions__ConnectionString` | `Host=postgres;...` | Direct connection (host = service name on the docker network). |
| `DatabaseOptions__MigrationsAssembly` | `DreamTeam.Migrations.PostgreSQL` | Central migrations project. |
| `CachingOptions__Redis` | `valkey:6379` | |
| `Storage__S3__ServiceUrl` | `http://minio:9000` | S3-compatible endpoint. |
| `Storage__S3__Bucket` | `dreamteam-uploads` | |
| `MailOptions__Smtp__Host` | `mailhog` | Dev only. |

## Port map

| Port | Service | Use |
|---|---|---|
| 5432 | postgres | Direct DB access (e.g. psql). |
| 6379 | valkey | Direct cache access. |
| 9000 | minio | S3 API. |
| 9001 | minio | Web console (default `minioadmin` / `minioadmin_dev`). |
| 1025 | mailhog | SMTP. |
| 8025 | mailhog | Web UI for inspecting outbound email. |
| 7030 | api (in container) | https-port-for-aspnetcore-https. |
| 3000 | web-stub | Future Nuxt 4 placeholder. |
| 11434 | ollama | OpenAI-compatible LLM API. |

## Quick commands

```bash
make up             # start base infra
make up-full        # also start api + web-stub
make logs           # tail logs
make migrate        # apply EF migrations
make psql           # open psql shell
make reset          # DESTRUCTIVE: drop volumes
```

## Production hardening (out of scope for this prep)

- Replace MinIO with real S3 (or compatible).
- Replace MailHog with real SMTP (SES, SendGrid, Mailgun, ...).
- Replace the dev `dreamteam_dev` secrets with secret-manager references.
- Enable TLS termination at the API host (or front it with nginx/traefik).
- Set `Storage__S3__PublicBaseUrl` to the CDN, not the bucket directly.
- Configure Postgres backups (see `docs/architecture.md` for the "self-host
  checklist" in the FDS — backup-restore procedure is tracked there).
