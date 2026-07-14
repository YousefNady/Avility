# Avility — Documentation

This folder contains the project's architecture decisions and API-related documentation.

## Structure

```
docs/
├── adr/                    Architecture Decision Records
└── api/
    └── API_TESTING_GUIDE.md   Full Swagger walkthrough (all endpoints, expected status codes)
```

## Architecture Decisions

ADRs live in [`adr/`](./adr/). Each ADR captures a significant architectural decision, its context, and its consequences (e.g. no generic repository, manual DTO mapping).

## API

- [`api/API_TESTING_GUIDE.md`](./api/API_TESTING_GUIDE.md) — ordered, end-to-end Swagger UI testing guide covering Auth, Profiles, Company Verification, Job Postings, Applications & Messaging, Resources, Admin, and cross-cutting checks.
