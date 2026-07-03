# ADR 0001: No generic repository / unit-of-work over EF Core

## Status
Accepted

## Context
Clean Architecture is often implemented with a generic `IRepository<T>` and
`IUnitOfWork` abstraction sitting between the Application layer and EF Core,
justified as "hiding EF Core from the Application layer" and "enabling
testability."

## Decision
Avility does not use a generic repository or unit-of-work abstraction.
Instead, the Application layer depends on a thin interface,
`IApplicationDbContext`, that exposes `DbSet<T>` properties and
`SaveChangesAsync`. MediatR command/query handlers use it directly.

## Reasoning
- `DbSet<T>` combined with `DbContext.SaveChangesAsync` already **is** a
  repository and unit-of-work implementation. Wrapping it in another
  interface duplicates behavior EF Core already provides, without adding
  capability.
- A generic repository typically ends up either too restrictive (no way to
  do `Include`, projection, or `AsNoTracking` cleanly) or leaks EF Core
  concepts back out through generic `Expression<Func<T, bool>>` parameters
  anyway — at which point it has provided no real abstraction.
- `IApplicationDbContext` is just as mockable/fakeable for unit tests as a
  generic repository would be, so testability is not a reason to add the
  extra layer.
- MediatR handlers already serve as the natural unit-of-work boundary: a
  handler loads what it needs, mutates it, and calls `SaveChangesAsync`
  once before returning.

## Consequences
- Handlers have direct, full access to EF Core's querying capabilities
  (`Include`, `AsNoTracking`, projections) without an abstraction getting
  in the way.
- If Avility ever needs to swap out EF Core entirely, that swap would
  require touching Application-layer handlers — considered an acceptable
  tradeoff, since this is not an anticipated requirement for this project.
