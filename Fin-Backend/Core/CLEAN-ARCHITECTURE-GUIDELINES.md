# Clean Architecture Structure

This document outlines the new clean architecture structure implemented for the Finmfb project.

## Overview

The project follows the principles of Clean Architecture, with distinct separation of concerns:

1. **Domain Layer**: Contains business entities, value objects, and domain logic
2. **Application Layer**: Contains application-specific business rules and use cases
3. **Infrastructure Layer**: Contains implementations of interfaces defined in the application layer
4. **API/Presentation Layer**: Contains controllers and presentation logic

## Folder Structure

```
/Core
  /Domain                 - Core business entities and logic
    /Entities             - Business entities organized by module
    /Common               - Common domain abstractions
    /Events               - Domain events
    /Exceptions           - Domain-specific exceptions
    /ValueObjects         - Value objects
    
  /Application            - Application services and interfaces
    /DTOs                 - Data Transfer Objects organized by module
    /Interfaces           - Service and repository interfaces
    /Services             - Service implementations
    /Features             - Feature modules (CQRS pattern)
      /Commands           - Command handlers
      /Queries            - Query handlers
    /Behaviors            - Cross-cutting behaviors like validation, logging
    /Mappings             - Object mapping profiles
    /Common               - Common application abstractions
    /Exceptions           - Application-specific exceptions
    
  /Infrastructure         - External services implementation
    /Persistence          - Database context and migrations
    /Repositories         - Repository implementations
    /Services             - External service implementations
    /Security             - Security implementations
    /Messaging            - Messaging and event handling
    /Caching              - Caching implementations
    
  /API                    - API/Presentation layer
    /Controllers          - API controllers organized by version
    /Middleware           - API middleware
    /Extensions           - Extension methods for the API
```

## Module Organization

Each business domain is organized into its own module:

1. **Accounting**: General ledger, journal entries, chart of accounts, financial reporting
2. **Auth**: Authentication, authorization, user management
3. **ClientPortal**: Client-facing functionality and services
4. **FixedAssets**: Fixed asset management, depreciation
5. **Loans**: Loan management, loan products, loan processing

## Migration Guidelines

When migrating code to the new structure:

1. Move domain entities to `/Core/Domain/Entities/{Module}`
2. Move DTOs to `/Core/Application/DTOs/{Module}`
3. Move service interfaces to `/Core/Application/Interfaces/Services`
4. Move repository interfaces to `/Core/Application/Interfaces/Repositories`
5. Move service implementations to `/Core/Application/Services` or as feature handlers
6. Move repository implementations to `/Core/Infrastructure/Repositories`
7. Update controllers to use the new services/features in `/Core/API/Controllers`

## Dependency Injection

Service registration should be organized by module in dedicated registration classes:

- `{Module}ServiceRegistration.cs` in the Application layer
- `{Module}InfrastructureRegistration.cs` in the Infrastructure layer

## Naming Conventions

- Entities: Singular nouns (e.g., `Account`, `JournalEntry`)
- DTOs: Suffix with "Dto" (e.g., `AccountDto`, `JournalEntryDto`)
- Interfaces: Prefix with "I" (e.g., `IAccountService`, `IGeneralLedgerRepository`)
- Repositories: Suffix with "Repository" (e.g., `AccountRepository`, `GeneralLedgerRepository`)
- Services: Suffix with "Service" (e.g., `AccountService`, `GeneralLedgerService`)
- Command/Queries: Use verb + noun (e.g., `CreateAccountCommand`, `GetAccountQuery`)