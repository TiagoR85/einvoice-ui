# ADR-0002: One-command hub compose lives here; shared demo OAuth client

- Status: Accepted
- Date: 2026-10-12

## Context

The family spans three app repos (`einvoice-api`, `einvoice-worker`,
`einvoice-ui`) plus infrastructure. The spec promises a **one-command demo**
(`docker compose up`) that boots everything. Two questions: where does that
compose file live, and which OAuth client does the UI use?

## Decision

1. The hub `docker-compose.yml` lives at the **root of this repo**. `api` and
   `worker` are built from their GitHub `main` via BuildKit git contexts
   (`context: https://github.com/TiagoR85/einvoice-{api,worker}.git#main`) —
   no meta-repo, no duplicated app code.
2. The UI authenticates to the API with the **shared demo client**
   `einvoice-client` (scopes `invoice:read invoice:write`, secret
   `OIDC_CLIENT_SECRET`). A production deployment would register a dedicated UI
   client; here the permission set would be identical, so a third client adds
   ceremony without security value (YAGNI — revisit when the demo goes to a VPS).

## Consequences

- `docker compose up` in this repo is the family's demo entry point; the other
  repos' READMEs point here for the full stack.
- Compromise of the UI process implies compromise of the demo client's
  read+write scope — acceptable for a local demo, explicitly not for production.
