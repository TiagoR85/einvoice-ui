# ADR-0001: Blazor Server, server-rendered

- Status: Accepted
- Date: 2026-10-12

## Context

The hub demo needs a UI that shows invoice lists, processing status and PDF
downloads. The audience is a local `docker compose up` demo and portfolio
reviewers reading the repo — not a public SaaS with millions of sessions.

## Decision

Use **Blazor Server** (.NET 10, interactive server rendering, Bootstrap from the
template) instead of a SPA (React/Vue) or Blazor WebAssembly:

- One language (C#) and one SDK across api/worker/ui — lower cognitive load for
  the study project and consistent tooling in CI.
- Server rendering keeps secrets (OAuth client secret, Silo credentials) on the
  server; the browser only ever talks to this app.
- No node toolchain, no separate frontend build — the existing reusable .NET CI
  template covers tests and coverage without extra jobs.

## Consequences

- Every user action is a SignalR round-trip; fine for a demo, wrong for huge
  scale (would need WebSocket-aware load balancing).
- UI instances are stateful per circuit; the demo runs a single instance.
