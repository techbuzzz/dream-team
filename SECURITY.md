# Security Policy

## Supported versions

DreamTeam is in active development. Only the current `main` branch receives security fixes.

## Reporting a vulnerability

**Do not open a public issue.** Use GitHub's private vulnerability reporting:

<https://github.com/victorbuzin/dream-team/security/advisories/new>

Please include:

- Affected component (module, file, endpoint)
- Reproduction steps and any required configuration
- Impact (what an attacker can achieve)
- Proof-of-concept if you have one

## What to expect

- Acknowledgement within 72 hours.
- Triage decision within 7 days.
- Coordinated disclosure window of ~90 days from triage, longer for changes that need careful migration paths.

Fixes ship as a patched commit on `main` plus a GitHub Security Advisory. Reporters are credited with permission.

## Scope

In scope: `src/` (BuildingBlocks, Modules, Host), `infra/`, `clients/`, and default `appsettings.*.json`.

Out of scope: third-party NuGet/npm packages (report upstream), the docs site, and issues in downstream forks (contact that fork's maintainer).

## Production hardening (before deploying a fork)

This scaffold ships with development-friendly defaults. Before deploying, rotate JWT signing keys, rotate the seeded demo password, lock CORS, set strong Hangfire dashboard credentials, and persist DataProtection keys to a shared store for multi-instance hosting. See `infra/README.md` for the production-readiness checklist.
