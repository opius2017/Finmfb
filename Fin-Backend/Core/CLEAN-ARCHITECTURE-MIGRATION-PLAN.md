# Clean Architecture Migration Plan

This document outlines the step-by-step plan for migrating the existing Finmfb codebase to the new clean architecture structure.

## Migration Steps

### 1. Setup Project Structure (Completed)

- Create clean architecture folder structure
- Setup base classes and interfaces in the Domain layer
- Create documentation for the architecture

### 2. Migrate Domain Layer

For each module:

- Move entities to `/Core/Domain/Entities/{Module}`
- Refactor entities to be self-contained with proper encapsulation
- Implement value objects for complex value types
- Add domain events for important state changes

### 3. Migrate Application Layer

For each module:

- Move DTOs to `/Core/Application/DTOs/{Module}` with each DTO in its own file
- Move service interfaces to `/Core/Application/Interfaces/Services`
- Move repository interfaces to `/Core/Application/Interfaces/Repositories`
- Implement service layer in `/Core/Application/Services` or as feature handlers
- Create proper mapping profiles in `/Core/Application/Mappings`

### 4. Migrate Infrastructure Layer

For each module:

- Implement repository pattern in `/Core/Infrastructure/Repositories`
- Update data access code to use the repository pattern
- Move external service implementations to `/Core/Infrastructure/Services`
- Configure dependency injection in `{Module}InfrastructureRegistration.cs`

### 5. Migrate API/Presentation Layer

For each module:

- Refactor controllers to use the new services in `/Core/API/Controllers`
- Implement API versioning
- Add proper validation and error handling
- Update Swagger documentation

### 6. Testing and Validation

- Create unit tests for the refactored components
- Ensure all existing functionality works with the new structure
- Validate that the clean architecture principles are being followed

## Module Migration Order

1. **Accounting Module** (In progress)
   - GeneralLedgerService (Completed)
   - ChartOfAccountService
   - JournalEntryService
   - FinancialReportingService

2. **Auth Module**
   - UserService
   - AuthenticationService
   - AuthorizationService

3. **ClientPortal Module**
   - ClientProfileService
   - ClientDashboardService
   - ClientNotificationService

4. **FixedAssets Module**
   - AssetService
   - DepreciationService
   - AssetDisposalService

5. **Loans Module**
   - LoanApplicationService
   - LoanProcessingService
   - LoanRepaymentService

## Tips for Refactoring

1. **One Entity/DTO Per File**: Each class should be in its own file with proper namespace
2. **Interface Segregation**: Break large interfaces into smaller, focused ones
3. **Dependency Inversion**: Depend on abstractions, not implementations
4. **Domain-Driven Design**: Focus on the domain model and business rules
5. **Command-Query Separation**: Separate commands (modify state) from queries (return data)

## Post-Migration Tasks

1. **Update Documentation**: Ensure all documentation reflects the new architecture
2. **Clean Up Legacy Code**: Remove redundant or obsolete code
3. **Performance Testing**: Verify the performance of the refactored system
4. **Knowledge Transfer**: Train the team on the new architecture