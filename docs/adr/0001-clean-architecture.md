# 0001. Clean architecture in four projects

**Status:** accepted

## Context
The API is small, but the alert logic must be testable without a database, and the database belongs to another project (the ETL).

## Decision
Four projects: `Domain`, `Application`, `Infrastructure`, `Api`. Use cases depend on interfaces (`IAirQualityReader`, `IAlertRuleStore`); EF Core appears only in `Infrastructure`. No MediatR: plain services are enough at this size.

## Consequences
- Business rules are tested with in-memory fakes (19 unit tests).
- More files than a single project; acceptable to keep the boundaries clear.
