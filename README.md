# einvoice-ui

[![ci](https://github.com/TiagoR85/einvoice-ui/actions/workflows/ci.yml/badge.svg)](https://github.com/TiagoR85/einvoice-ui/actions/workflows/ci.yml)
[![license](https://img.shields.io/badge/license-MIT-green)](LICENSE)

Blazor Server UI for the `einvoice-hub` (EU e-invoicing) — list and create invoices, watch processing status live, download the generated PDF. This repo is also the **one-command demo entry point** for the whole family: `docker compose up` here boots Postgres, Redis, RabbitMQ, Silo, [`einvoice-api`](https://github.com/TiagoR85/einvoice-api), [`einvoice-worker`](https://github.com/TiagoR85/einvoice-worker) and the UI.

## About this repository

This is a **study project** from my public portfolio. I learn by building: I use **AI-assisted development tools** (agent-based pair programming) while holding the project to production standards - automated tests, CI quality gates, security scanning, review pipelines and documented architecture decisions.

The scenario is simple on purpose; the engineering is not.

## How it fits the hub

```
Browser ──▶ einvoice-ui (Blazor Server, :8081)
              │ REST + OAuth2 client-credentials (client einvoice-client)
              ▼
           einvoice-api (:8080) ──outbox──► RabbitMQ ──► einvoice-worker ──► Silo (:9000)
              ▲                                                 │ invoice.completed.v1
              └────────────── inbox consumer ◀──────────────────┘
                     status flips Issued → Completed; UI shows the badge and the PDF link
```

- The UI authenticates with the **shared demo client** `einvoice-client` (read+write) — see [ADR-0002](docs/adr/ADR-0002-hub-demo-orchestration.md).
- PDF downloads are proxied server-side from Silo (`GET /pdf/{invoiceId}`) — see [ADR-0003](docs/adr/ADR-0003-pdf-download-proxy.md).
- Status flips when the API's inbox consumer (in `einvoice-api`) handles `invoice.completed.v1` — see [ADR-0001](docs/adr/ADR-0001-blazor-server.md).

## Quickstart — one command (the whole hub)

Prerequisites: Docker Desktop with BuildKit (default), ~4 GB free RAM.

```powershell
git clone https://github.com/TiagoR85/einvoice-ui.git
cd einvoice-ui
Copy-Item .env.example .env   # set OIDC_CLIENT_SECRET, OIDC_WORKER_CLIENT_SECRET, POSTGRES_PASSWORD
docker compose up --build     # first run builds api/worker from GitHub (~3-5 min)
```

> If the build fails with `failed to evaluate path "https://github.com/..."`, your Docker Desktop compose bake path is mishandling git-URL contexts — rerun as `COMPOSE_BAKE=false docker compose up --build` (plain `docker build` with the same git URL also works).

Then open **http://localhost:8081**:

1. **Parties** → create a Supplier and a Customer.
2. **New invoice** → fill number/dates/lines, pick seller/buyer, submit.
3. **Invoices** → status starts `Issued`; within seconds the worker renders the PDF and the badge flips to `Completed` (auto-refresh is on) — the **PDF** button downloads it.

Useful endpoints while the demo runs: API `http://localhost:8080/health/live`, Silo console `http://localhost:9001` (minioadmin/minioadmin when the env vars are empty), RabbitMQ management `http://localhost:15672` (guest/guest).

## UI-only development

With the hub deps from [`einvoice-api`](https://github.com/TiagoR85/einvoice-api)'s compose running on the host (`docker compose up` there):

```powershell
dotnet run --project src\Einvoice.Ui   # http://localhost:8081 per launchSettings
```

## Configuration

| Key | Default | Notes |
|---|---|---|
| `Api:BaseUrl` | http://localhost:8080 | einvoice-api base URL |
| `Api:ClientId/ClientSecret/Scope` | einvoice-client/(env)/invoice:read invoice:write | OAuth2 client-credentials |
| `Minio:Endpoint/AccessKey/SecretKey/Bucket` | localhost:9000/minioadmin/minioadmin/einvoice-docs | Silo (MinIO-compatible) for PDFs |

Secrets come only from environment variables (`.env`, gitignored); `.env.example` documents keys without values.

> **Note on object storage:** the compose uses [`pgsty/silo`](https://hub.docker.com/r/pgsty/silo), a community MinIO-compatible (S3) server — the original `minio/minio` image was removed from Docker Hub in Sep 2026 when MinIO went source-only.

## Tests

```powershell
dotnet test tests\Einvoice.Ui.Tests\Einvoice.Ui.Tests.csproj /p:CollectCoverage=true /p:Threshold=80 /p:ThresholdType=line
```

Unit tests (services, bUnit component tests) run in milliseconds; one integration test boots a real Silo container (Testcontainers) and a WireMock stub of the API. Coverage gate: 80% line (enforced in CI).

## Decisions

- [ADR-0001](docs/adr/ADR-0001-blazor-server.md) — Blazor Server, server-rendered, no SPA
- [ADR-0002](docs/adr/ADR-0002-hub-demo-orchestration.md) — one-command hub compose lives here; shared demo OAuth client
- [ADR-0003](docs/adr/ADR-0003-pdf-download-proxy.md) — PDFs proxied through the UI from Silo

## Roadmap

- [x] Blazor pages: invoices, invoice creation, parties
- [x] Processing status via `invoice.completed.v1` inbox in the API
- [x] One-command hub demo (`docker compose up`)
- [ ] VPS deployment of the hub demo (future)

## License

MIT
