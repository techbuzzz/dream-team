# Contributing

Thanks for helping out. The conventions below keep PRs reviewable.

## Reporting issues

- **Security:** Use GitHub's private advisories — see [SECURITY.md](SECURITY.md). Do not file public issues for vulnerabilities.
- **Bugs:** Open a [GitHub issue](https://github.com/victorbuzin/dream-team/issues) with a minimal repro, your .NET SDK version, and the DB provider.
- **Features:** Start a [Discussion](https://github.com/victorbuzin/dream-team/discussions) before opening a PR for non-trivial work.

## Dev setup

Prerequisites: .NET 10 SDK, Docker, Node.js 20+.

```bash
# Whole stack (Aspire orchestrator)
dotnet run --project src/Host/DreamTeam.AppHost

# Or with Docker only (the infra/ services, run the API on the host)
make -C infra up
dotnet run --project src/Host/DreamTeam.Api

# Build and test
dotnet build src/DreamTeam.slnx
dotnet test  src/DreamTeam.slnx   # integration suite needs Docker
```

Client apps live under `clients/admin` and `clients/dashboard` — `npm install && npm run dev` in each. The Nuxt 4 SPA in `apps/web` is a separate workstream (post-MVP-1).

## Pull requests

- Branch from and target `main`.
- Follow [Conventional Commits](https://www.conventionalcommits.org) — match the existing history (`feat(forms): ...`, `fix(identity): ...`).
- Add tests. The build runs with `TreatWarningsAsErrors=true`; analyzer warnings must be fixed.
- Don't touch `src/BuildingBlocks/` without prior discussion — wide blast radius (the FSH golden rule, still applies).
- Architecture rules (module boundaries, file layout, coding style) are documented in [AGENTS.md](AGENTS.md). Apply them.

## Code of conduct

This project follows the [Contributor Covenant](CODE_OF_CONDUCT.md).

## Licensing

Contributions are licensed under the project's [LICENSE](LICENSE) (TBD; see top-level README).
