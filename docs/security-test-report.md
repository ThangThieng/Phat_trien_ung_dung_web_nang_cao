# Buổi 8 — Security test report

Environment: isolated official Compose project `culinaryblog_buoi8_quan_official` on 2026-10-31. Runtime entrypoint was `127.0.0.1:3300`; direct API was `127.0.0.1:5300`. No main-stack service was used. Values that could authenticate a user or reset a password are intentionally redacted and never recorded here.

| Security check | Objective and method | Expected / actual | Evidence | Result |
| --- | --- | --- | --- | --- |
| X-Forwarded-For trust | Direct API requests supplied two different spoofed `X-Forwarded-For` values. The production pipeline was also exercised with trusted and untrusted remote addresses. | Direct requests both returned 401; spoofed XFF did not authenticate or impersonate a client. Untrusted XFF cannot evade the quota; trusted proxy forwarding partitions by the forwarded client IP. | `SecurityMiddlewareTests.UntrustedForwardedIpCannotEvadeQuota`, `TrustedRealIpPartitionsRemainIndependent`; configuration permits only `172.16.0.0/12`. | PASS |
| Auth rate limit | Production-default pipeline test sent 11 requests to each Auth route without the scratch E2E override. | Requests 1–10 were processed; request 11 was 429 with `Retry-After`, rate-limit headers, and `RATE_LIMIT_EXCEEDED`. | `SecurityMiddlewareTests.ProductionQuotaAndRejectionContract` (39 focused backend security tests passed). | PASS |
| Hangfire authorization | Unauthenticated GET to `/hangfire` on direct API and reverse proxy. | Direct: 401. Proxied: 401. | Scratch runtime checks on `5100` and `3100`; nginx Basic Auth plus application gate remain enabled. | PASS |
| Author authorization | Use an Author role against user administration endpoints and client role guard. | `GET /api/v1/users`: 403; `PATCH /api/v1/users/{id}/status`: 403; Admin dashboard guard renders the 403 UI for Author. | `AuthorCannotListUsersOrChangeAccountStatus`; `RequireRole.test.tsx`. Foreign-resource edit is not a Dev1 endpoint contract in this checklist run. | PASS |
| `/auth/me` DTO privacy | Authenticated profile response is inspected only for field names. | No `PasswordHash`, `SecurityStamp`, `ConcurrencyStamp`, refresh-token/hash, or other Identity internal fields. | `GetMe_ReturnsProfileWithoutSensitiveFields_AndNoStore`. | PASS |
| `/auth/sessions` DTO privacy | Authenticated session list is inspected only for field names. | No token hash, raw refresh token, or raw access token. | `GetSessions_MarksCurrentSessionBySid_AndNeverExposesTokenHash`. | PASS |
| Forgot-password enumeration | Compare a registered and an unknown email through the public endpoint. | Both return equivalent 202 Accepted responses with the same public body/media shape. | `ForgotPassword_ReturnsEquivalentAcceptedResponseForExistingAndUnknownEmail`. | PASS |
| Reset URL cleanup | Execute the real scratch MailHog reset flow. | Reset input is accepted; the visible URL is cleaned and no reset token is retained in browser storage. | `frontend/e2e/auth.spec.ts` Flow 5 (previous Auth E2E gate: 5/5). | PASS |
| Open redirect | Exercise unsafe callback inputs and a safe internal path. | External, protocol-relative, backslash, and script/data inputs fall back to `/dashboard`; a local path retains query/hash. Browser regression confirms external callback falls back safely. | `safe-redirect.test.ts`; `frontend/e2e/csp.spec.ts`. | PASS |
| CSP | Browser checks login, register, forgot/reset password, dashboard users, profile, and recipes. | Zero application CSP violations after allowing only exact, version-pinned Sonner stylesheet hashes; no `unsafe-inline` in production `style-src`. | `frontend/e2e/csp.spec.ts`: 4/4 passed; `csp.test.ts`: 2/2 passed. | PASS |
| Secret-value log scan | Scan last two hours of isolated API, Hangfire, nginx, and Seq container logs for credential/token-value patterns. | No secret-value matches in any source. Benign words such as “token” were not treated as leaks. | Scratch-only pattern scan: API/Hangfire/nginx/Seq each 0 matches. | PASS |
| Browser token storage | Verify current persisted auth state implementation and its regression test. | Access token is memory-only. The refresh token is intentionally persisted in `localStorage` for reload recovery and cross-tab rotation coordination; this is the approved architecture, not a Buổi 8 failure. | README Lab 6 Dev 1: `partialize` persists only `{ refreshToken, user }`; `SRS_MAU_THUAN_VA_GIAI_PHAP.md` MT-55 decision A; `src/stores/auth-store.ts`. | PASS |

## Automated regression summary

- Backend security and policy focus: 63 passed, 0 failed, 0 skipped.
- Added enumeration-safe forgot-password integration regression plus profile/session/Author focus: 4 passed, 0 failed, 0 skipped.
- Frontend storage, role guard, safe redirect, and forgot-password UI focus: 13 passed, 0 failed.
- CSP browser suite: 4 passed, 0 failed.

## Storage-contract clarification

The current source of truth explicitly approves the chosen split: access token in memory only, and the refresh token as the only durable browser token. Moving it to an HttpOnly cookie would change the established backend contract and requires a separate Change Request; it is not a remediation item for this checklist.
