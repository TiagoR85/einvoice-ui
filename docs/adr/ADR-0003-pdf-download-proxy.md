# ADR-0003: PDFs proxied through the UI from Silo

- Status: Accepted
- Date: 2026-10-12

## Context

The worker stores generated invoice PDFs in Silo (MinIO-compatible S3) under
`invoices/{invoiceId}.pdf` in bucket `einvoice-docs`. The UI must offer a
download link. Options: presigned S3 URLs, exposing Silo publicly, or a
server-side proxy route.

## Decision

Serve downloads through a minimal API route in this app:
`GET /pdf/{invoiceId}` → `IDocumentClient` (Minio SDK, server-side credentials)
→ `Results.File(bytes, "application/pdf")` or 404.

Presigned URLs were rejected because they embed the **in-network hostname**
(`minio:9000`) that the browser cannot resolve; exposing Silo publicly would
hand anonymous callers an unauthenticated S3 API. The proxy keeps Silo fully
internal and lets the UI enforce "only Completed invoices have a link" in the
page (the route itself is idempotent read — safe to expose).

## Consequences

- PDF bytes flow through the UI process — fine for demo-sized documents.
- If PDFs grow large or traffic increases, switch to short-TTL presigned URLs
  generated with an externally reachable endpoint.
