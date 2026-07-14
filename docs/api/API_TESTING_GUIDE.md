# Avility — Full Swagger Testing Guide

A complete, ordered walkthrough of every endpoint via Swagger UI, with expected status codes and *why*. Designed to be run once, top to bottom, before recording/screenshotting for your LinkedIn post.

## Before you start

**Use multiple browser profiles/incognito windows — one per actor.** You'll need 5 identities active at once (Admin, two Companies, two JobSeekers) for the ownership/authorization tests to make sense. Swapping one token in and out of Swagger's single "Authorize" button constantly is error-prone. Recommended: 5 separate browser windows (or profiles), each logged in as one actor, tabs pinned:

| Actor | Purpose |
|---|---|
| **Admin** | your seeded `Seed:AdminEmail`/`Seed:AdminPassword` account |
| **Company A** | the "real" company that owns postings/applications you'll test the happy path with |
| **Company B** | exists only to prove Company A's data is *not* accessible to it (403 checks) |
| **JobSeeker A** | the "real" applicant |
| **JobSeeker B** | exists only to prove JobSeeker A's data is *not* accessible to it (403 checks) |

In each window: click **Authorize** in Swagger UI, paste that actor's `accessToken` (from their login response), confirm. Every "Try it out" call in that window now carries that actor's identity.

**Keep a scratch pad** of IDs as you go — you'll need `jobPostingId`, `jobApplicationId`, `companyId`, `resourceId` repeatedly across phases.

---

## Phase 0 — Health (no auth, run first)

| # | Call | Expect |
|---|---|---|
| 0.1 | `GET /health` | **200**, JSON body, `status: "Healthy"`, both `database` and `file-storage` checks Healthy |
| 0.2 | `GET /health/live` | **200**, `checks: []` (empty — by design, no dependency checks) |
| 0.3 | `GET /health/ready` | **200**, `checks` contains `database` + `file-storage`, both Healthy |

---

## Phase 1 — Auth

Do this in **each** of the 5 browser windows, using a distinct email per actor (e.g. `admin@yourtest.com` is your real seeded one; `companyA@test.com`, `companyB@test.com`, `seekerA@test.com`, `seekerB@test.com` — all fresh emails, never registered before).

### 1.1 Register — Company A
`POST /auth/register`
```json
{ "email": "companyA@test.com", "password": "Password123!", "role": "Company" }
```
**Expect: 200**, body contains `accessToken` + `refreshToken`.

Repeat identically for **Company B**, **JobSeeker A** (`role: "JobSeeker"`), **JobSeeker B**.

### 1.2 ⚠️ Verify: registering as Admin is blocked
`POST /auth/register`
```json
{ "email": "shouldnotwork@test.com", "password": "Password123!", "role": "Admin" }
```
**Expect: 400** (validation should reject `Admin` as a self-registerable role — Admin should only exist via seeding, never via public self-registration). **If this returns 200 and actually creates an Admin account, stop and flag it to me — that's a real security bug, not an expected pass.** I'm not 100% certain from memory which way this currently behaves, which is exactly why it's worth checking before you publish anything.

### 1.3 Login — Admin (your seeded account)
`POST /auth/login`
```json
{ "email": "<your real seeded admin email>", "password": "<your real seeded admin password>" }
```
**Expect: 200.** This is your one and only way to get an Admin token — there is no "promote to Admin" endpoint anywhere in the API by design.

### 1.4 Login — wrong password
`POST /auth/login` with Company A's email, wrong password.
**Expect: 400** (not 401 — invalid credentials are treated as a validation failure, and deliberately don't distinguish "wrong password" from "no such user," to avoid leaking which emails are registered).

### 1.5 Refresh token
`POST /auth/refresh` with Company A's real `refreshToken`.
**Expect: 200**, new `accessToken` + new `refreshToken` (rotation — the old refresh token is now revoked).

### 1.6 Refresh with the now-revoked (old) token
`POST /auth/refresh` reusing the *original* refresh token from 1.1 (not the new one from 1.5).
**Expect: 400** (already rotated/revoked).

### 1.7 Forgot password — real email
`POST /auth/forgot-password`
```json
{ "email": "companyA@test.com" }
```
**Expect: 200** regardless.

### 1.8 Forgot password — nonexistent email
`POST /auth/forgot-password`
```json
{ "email": "definitely-not-registered@nowhere.com" }
```
**Expect: 200** — same response as 1.7. This is deliberate (anti-account-enumeration): the endpoint never reveals whether an email exists. If you have SMTP access, only 1.7 actually sends an email; 1.8 silently does nothing but still returns 200.

### 1.9 Logout
`POST /auth/logout` with a valid refresh token.
**Expect: 200.**

---

## Phase 2 — Profiles

### 2.1 JobSeeker profile — create (as JobSeeker A)
`POST /jobseekers/me`
```json
{
  "fullName": "Test Seeker",
  "phoneNumber": "+201234567890",
  "yearsOfExperience": 3,
  "currentJobTitle": "QA Engineer",
  "country": "Egypt",
  "governorate": "Cairo",
  "city": "Cairo",
  "disabilityCategories": ["Visual"],
  "accommodationNotes": "Prefers screen-reader-compatible tools."
}
```
**Expect: 200.**

### 2.2 JobSeeker profile — get mine (as JobSeeker A)
`GET /jobseekers/me` → **200**, matches what you just created.

### 2.3 JobSeeker profile — get mine, no profile yet (as JobSeeker B)
`GET /jobseekers/me` → **Expect: 404** (JobSeeker B hasn't created a profile yet).

### 2.4 ⚠️ JobSeeker profile — create a second time (as JobSeeker A)
`POST /jobseekers/me` again with the same body.
**Expect: some kind of 400.** I'm genuinely not certain whether this is caught cleanly by application logic or falls through to the database's unique constraint — **if you get a 500 instead of a 400 here, tell me**, that's a real gap worth fixing before you publish (a 500 on a duplicate-create is exactly the kind of thing a sharp viewer might try first).

### 2.5 JobSeeker profile — update (as JobSeeker A)
`PUT /jobseekers/me` with a changed field (e.g. `yearsOfExperience: 4`).
**Expect: 200**, reflects the change on a follow-up GET.

### 2.6 Resume upload (as JobSeeker A)
⚠️ Find the exact route in Swagger under the **JobSeekers** group (something like `POST /jobseekers/me/resume`) — use "Try it out," attach a real `.pdf`.
**Expect: 200.**

### 2.7 Resume upload — wrong file type
Same endpoint, attach a `.png` or `.jpg`.
**Expect: 400** (content-type whitelist: pdf/doc/docx only).

### 2.8 Resume download (as JobSeeker A)
`GET` the resume endpoint — **Expect: 200**, browser/Swagger should offer the file, byte-identical to what you uploaded.

### 2.9 Company profile — create (as Company A)
`POST /companies/me`
```json
{
  "companyName": "Test Company A",
  "companySize": "ElevenToFifty",
  "foundedYear": 2018,
  "country": "Egypt",
  "governorate": "Giza",
  "city": "Giza"
}
```
**Expect: 200.**

Repeat for **Company B** with a distinct `companyName`.

### 2.10 Company profile — get mine (as Company A)
`GET /companies/me` → **200**. Note the `id` returned — this is your `companyId`.

### 2.11 ⚠️ Company logo upload/download
Find the logo endpoints under the **Companies** group in Swagger (this feature was built separately from this test guide — I don't have the exact route names memorized). Test upload with a real image, then download, and confirm it round-trips. If anything here 404s or behaves unexpectedly, that's useful to know before publishing, since I can't vouch for this part from direct review.

---

## Phase 3 — Admin: Company Verification

### 3.1 Attempt to publish a job as an unverified company — *(do this after Phase 4.1, comes back to it)*
Skip for now — flagged here so you remember the order matters (see 4.3).

### 3.2 List companies by verification status (as Admin)
`GET /admin/companies?status=Pending` → **200**, should include both Company A and Company B (fresh registrations start `Pending`).

### 3.3 Verify Company A (as Admin)
`POST /companies/{companyA_id}/verify` → **200.**

### 3.4 Verify Company A again — as Company A (wrong role)
Same call, but using Company A's own token instead of Admin's.
**Expect: 403** (Admin-only action).

### 3.5 Verify Company A — no token at all
Same call, Authorize cleared / no header.
**Expect: 401.**

### 3.6 Leave Company B unverified deliberately
You'll use this in Phase 4 to prove the verification gate actually works. Don't verify Company B yet.

---

## Phase 4 — Job Postings

### 4.1 Create a posting (as Company A) — should work even though not yet... wait, Company A *is* verified now (3.3). Create as Company B instead first to test the gate properly:

**As Company B** (still unverified):
`POST /jobpostings`
```json
{
  "title": "Support Engineer",
  "description": "Remote support role.",
  "employmentType": "FullTime",
  "experienceLevel": "MidLevel",
  "isRemote": true
}
```
**Expect: 200** — creating a **Draft** posting does *not* require verification; only *publishing* does. This is a subtle, deliberate rule worth demonstrating.

### 4.2 Publish that posting as unverified Company B
`POST /jobpostings/{that_posting_id}/publish`
**Expect: 400** — "company must be verified" business rule. This is the actual verification gate in action.

### 4.3 Create + publish a posting as verified Company A
`POST /jobpostings`
```json
{
  "title": "Accessible Backend Engineer",
  "description": "Remote-friendly backend role with full accommodation support.",
  "employmentType": "FullTime",
  "experienceLevel": "MidLevel",
  "isRemote": true,
  "applicationDeadline": "2027-01-01T00:00:00Z",
  "supportedDisabilityCategories": ["Visual"],
  "accommodationDetails": "Screen-reader compatible tooling, flexible hours."
}
```
**Expect: 200** (Draft). Save this `jobPostingId`.

`POST /jobpostings/{jobPostingId}/publish` → **Expect: 200** (Company A is verified).

### 4.4 Publish with a past deadline
Create a second posting, `"applicationDeadline": "2020-01-01T00:00:00Z"`, then try to publish it.
**Expect: 400.**

### 4.5 Publish an already-published posting
`POST /jobpostings/{jobPostingId}/publish` again on the 4.3 posting.
**Expect: 400** (already Published — invalid transition).

### 4.6 Search — public, anonymous
Clear your Authorize token entirely for this one call (or use a fresh incognito tab with no auth).
`GET /jobpostings` → **200**, should include the 4.3 posting.

### 4.7 Search — filtered by accessibility category
`GET /jobpostings?disabilityCategory=Visual` → **200**, includes the 4.3 posting.
`GET /jobpostings?disabilityCategory=Mobility` → **200**, likely excludes it (unless you tagged it Mobility too).

### 4.8 Search — invalid category value
`GET /jobpostings?disabilityCategory=NotARealCategory` → **Expect: 400.**

### 4.9 Get by ID — anonymous, Published posting
`GET /jobpostings/{jobPostingId}` (no auth) → **200.**

### 4.10 Get by ID — anonymous, Draft posting
Use the Draft posting ID from 4.4 (never published). `GET /jobpostings/{draftId}` (no auth) → **Expect: 404**, not 403 — drafts are invisible, not merely forbidden.

### 4.11 Update a Published posting
`PUT /jobpostings/{jobPostingId}` as Company A → **200**, allowed.

### 4.12 Close, then try to update
`POST /jobpostings/{jobPostingId}/close` → **200.**
Then `PUT /jobpostings/{jobPostingId}` again → **Expect: 400** (Closed postings can't be edited at all).

### 4.13 "Mine" — as the wrong company
`GET /jobpostings/mine` as **Company B** → **200**, but should **not** include Company A's postings (only Company B's own).

### 4.14 Recommended — as JobSeeker A
`GET /jobpostings/recommended` → **200**, the 4.3 posting (tagged `Visual`) should rank at or near the top, since JobSeeker A disclosed `Visual` too (Phase 2.1).

### 4.15 Recommended — as a Company (wrong role)
Same call with Company A's token.
**Expect: 403.**

---

## Phase 5 — Job Applications & Messaging

Use the 4.3 posting (Published, owned by Company A) for all of this.

### 5.1 Apply — as JobSeeker A
`POST /jobapplications`
```json
{ "jobPostingId": "{jobPostingId}", "coverLetter": "I'd love to work here." }
```
**Expect: 200.** Save `jobApplicationId`.

### 5.2 Apply a second time — same JobSeeker, same posting
Same call again.
**Expect: 400** (already applied — the unique-application rule).

### 5.3 Apply to a Draft posting
Use the unpublished posting from 4.4/4.10 instead.
**Expect: 400** (can only apply to Published postings).

### 5.4 "Mine" — as JobSeeker B (should be empty of A's application)
`GET /jobapplications/mine` as JobSeeker B → **200**, does not include the 5.1 application.

### 5.5 Withdraw — as JobSeeker A
`POST /jobapplications/{jobApplicationId}/withdraw` → **200.**

### 5.6 Reapply after withdrawal
`POST /jobapplications` with the same body as 5.1 again.
**Expect: 400** (no reapplying after withdrawal — same row, still blocks it).

### 5.7 Withdraw — as the wrong person (Company A, not the JobSeeker)
`POST /jobapplications/{jobApplicationId}/withdraw` using Company A's token.
**Expect: 403.**

### 5.8 Fresh application for the accept/reject flow
Apply again as **JobSeeker B** to the same posting (a fresh application, since A's is now withdrawn):
`POST /jobapplications` as JobSeeker B → **200.** Save this new `jobApplicationId`.

### 5.9 Under review — as Company A
`POST /jobapplications/{jobApplicationId}/under-review` → **200.**

### 5.10 Accept — as Company A
`POST /jobapplications/{jobApplicationId}/accept` → **200.** This should trigger a background email to JobSeeker B (check your SMTP/inbox if you want to confirm, or check `/admin/test-email` separately in Phase 7 instead if you don't want to wait).

### 5.11 Accept — as the wrong company (Company B)
Same call, Company B's token.
**Expect: 403.**

### 5.12 Reject an already-Accepted application
`POST /jobapplications/{jobApplicationId}/reject` (still Company A).
**Expect: 400** (Accepted is terminal).

### 5.13 Messages — send, as a participant (JobSeeker B)
`POST /jobapplications/{jobApplicationId}/messages`
```json
{ "body": "Thank you for accepting my application!" }
```
**Expect: 200.**

### 5.14 Messages — send, as the other participant (Company A)
Same endpoint, Company A's token.
```json
{ "body": "Welcome aboard - let's schedule an intro call." }
```
**Expect: 200.**

### 5.15 Messages — get thread, as either participant
`GET /jobapplications/{jobApplicationId}/messages` → **200**, both messages, oldest first.

### 5.16 Messages — send/read as a non-participant (JobSeeker A, or Company B)
Same endpoints, wrong actor's token.
**Expect: 403** on both send and read.

### 5.17 Messages — real-time (not testable via Swagger)
Swagger can't open a WebSocket connection to `/hubs/messages`. If you want to demo real-time delivery for the LinkedIn post specifically, that needs a small script or a tool like Postman's WebSocket support — not something to attempt through Swagger UI. Let me know if you want a minimal test-client page for this.

---

## Phase 6 — Resources

### 6.1 Create — as Admin
`POST /resources`
```json
{
  "title": "Writing an Accessible Resume",
  "description": "A practical guide for job seekers with disabilities.",
  "url": "https://example.com/accessible-resume-guide",
  "category": "ResumeWriting"
}
```
**Expect: 200.** Save `resourceId`.

### 6.2 Create — as a non-admin
Same call, any Company/JobSeeker token.
**Expect: 403.**

### 6.3 Search — anonymous
`GET /resources` (no auth) → **200**, includes the new resource.

### 6.4 Search — filtered
`GET /resources?category=ResumeWriting` → **200**, includes it.
`GET /resources?category=Other` → likely excludes it.

### 6.5 Get by ID — anonymous
`GET /resources/{resourceId}` → **200.**

### 6.6 Update — as Admin
`PUT /resources/{resourceId}` with a changed `title`.
**Expect: 200.**

### 6.7 Delete — as Admin
`DELETE /resources/{resourceId}` → **200.**

### 6.8 Get the deleted resource
`GET /resources/{resourceId}` → **Expect: 404** (soft-deleted — invisible via the global query filter, same as a Draft posting to an anonymous caller).

---

## Phase 7 — Admin Dashboard & Ops

### 7.1 Platform statistics
`GET /admin/statistics` (Admin) → **200**, numbers should roughly match what you've created this session (≥2 JobSeekers, ≥2 Companies, ≥1 Published posting, ≥1 application, etc).

### 7.2 Statistics — wrong role
Same call, any non-Admin token.
**Expect: 403.**

### 7.3 Test email
`POST /admin/test-email`
```json
{ "toEmail": "<an inbox you can check>" }
```
**Expect: 200** ("Test email dispatched..."). Check the inbox (and spam) shortly after to confirm real SMTP delivery.

### 7.4 Admin actions — no token at all
Try any `/admin/*` call with Authorize cleared.
**Expect: 401** (not 403 — no credentials at all is unauthenticated, not merely forbidden).

---

## Phase 8 — Cross-Cutting Checks (bonus, worth a screenshot or two)

### 8.1 Malformed JSON body
Pick any `POST` endpoint, manually break the JSON in the request body editor (e.g. delete a closing brace) before sending.
**Expect: 400**, and the response body should still be your standard `{ "success": false, "message": "...", "errors": {...} }` envelope — **not** the raw ASP.NET Core default error shape. This is worth a screenshot specifically — it's a subtle but real correctness detail.

### 8.2 Response headers
Open the response panel (Swagger UI shows response headers under "Try it out" results) on any call and check for:
- `X-Correlation-Id` — should be present on literally every response
- `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy: strict-origin-when-cross-origin`

### 8.3 Swagger's own security integration
Click the **Authorize** button itself — confirm it shows a proper Bearer/JWT scheme with a description, not an empty/unlabeled auth box. This was a real gap fixed specifically for this milestone (public demo readiness).

---

## Summary checklist

- [ ] Phase 0 — Health
- [ ] Phase 1 — Auth (including the Admin-role-blocking check, 1.2)
- [ ] Phase 2 — Profiles (including the duplicate-create check, 2.4, and logo, 2.11)
- [ ] Phase 3 — Company verification
- [ ] Phase 4 — Job postings (including the unverified-publish gate, 4.2)
- [ ] Phase 5 — Applications + messaging
- [ ] Phase 6 — Resources
- [ ] Phase 7 — Admin dashboard + test email
- [ ] Phase 8 — Cross-cutting (correlation ID, security headers, error envelope)

If anything in the ⚠️-flagged items comes back differently than expected, tell me exactly what you saw (status code + response body) and I'll either explain why it's actually correct or we'll fix it as a real bug before you publish.
