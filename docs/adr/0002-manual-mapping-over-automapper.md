# ADR 0002: Manual DTO mapping by default, not AutoMapper

## Status
Accepted

## Context
The original tech stack listed AutoMapper as an available tool, with the
caveat "only where it adds value."

## Decision
Avility maps entities to DTOs with explicit `ToDto()` extension methods by
default. AutoMapper is not installed at project start and will only be
introduced if a specific mapping becomes genuinely repetitive or complex
enough that the manual version is a real maintenance burden.

## Reasoning
- With roughly five entities and straightforward DTO shapes, a reviewer
  reading `public static JobSeekerDto ToDto(this JobSeeker e) => new(...)`
  understands the mapping immediately.
- AutoMapper's convention-based profiles require the reader to trust that
  the conventions match, and hide the actual mapping behind reflection —
  harder to debug and harder to explain in a code review or interview.
- YAGNI: the library has not yet earned its place by solving a real
  repetition problem.

## Consequences
- Slightly more boilerplate per entity in exchange for mappings that are
  explicit and trivially debuggable.
- If a specific entity's mapping becomes large and repetitive later, this
  decision can be revisited for that entity specifically, without
  requiring AutoMapper project-wide.
