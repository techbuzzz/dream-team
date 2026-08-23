# DreamTeam — convenience targets for infra/.
#
# Usage:
#   make help
#   make up        # start the base infra (postgres, valkey, minio, mailhog)
#   make up-full   # also start the api + web-stub containers
#   make down      # stop everything
#   make logs      # tail logs
#   make reset     # WARNING: drops all infra data volumes
#   make migrate   # apply pending EF migrations
#   make pull-ollama  # start the optional ollama service for AI digest (MVP-2)

COMPOSE = docker compose -f infra/docker-compose.yml
COMPOSE_FULL = $(COMPOSE) -f infra/docker-compose.ollama.yml

.PHONY: help up up-full down logs reset migrate seed pull-ollama psql redis-cli

help:
	@echo "DreamTeam infra targets:"
	@echo "  make up          - start base infra (postgres, valkey, minio, mailhog)"
	@echo "  make up-full     - also start api + web-stub"
	@echo "  make down        - stop everything (keeps volumes)"
	@echo "  make logs        - tail logs"
	@echo "  make reset       - DESTRUCTIVE: drop volumes, fresh DB"
	@echo "  make migrate     - apply EF migrations"
	@echo "  make pull-ollama - start optional ollama service for AI digest"
	@echo "  make psql        - open a psql shell in the postgres container"
	@echo "  make redis-cli   - open valkey-cli in the valkey container"

up:
	$(COMPOSE) up -d

up-full:
	$(COMPOSE) --profile full up -d

down:
	$(COMPOSE) down

logs:
	$(COMPOSE) logs -f

reset:
	$(COMPOSE) down -v
	@echo "Volumes dropped. Run 'make up' to start fresh."

migrate:
	cd src/Host/DreamTeam.DbMigrator && dotnet run -- apply

seed:
	cd src/Host/DreamTeam.DbMigrator && dotnet run -- seed

pull-ollama:
	$(COMPOSE_FULL) up -d ollama ollama-pull

psql:
	docker exec -it dreamteam-postgres psql -U dreamteam -d dreamteam

redis-cli:
	docker exec -it dreamteam-valkey valkey-cli
