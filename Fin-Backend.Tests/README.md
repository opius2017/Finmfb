# FinMFB Banking Application Testing

This directory contains comprehensive tests for the FinMFB Banking Application.

## Test Structure

The test project is organized into the following sections:

- **Unit Tests**: Test individual components in isolation, mocking dependencies
- **Integration Tests**: Test interaction between components
- **Functional Tests**: Test complete features end-to-end
- **Common**: Contains shared test utilities and fixtures

## Running Tests

### Using Visual Studio

1. Open the solution in Visual Studio
2. Use Test Explorer to run tests

### Using Command Line

```bash
# Run all tests
dotnet test

# Run specific test category
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
dotnet test --filter "Category=Functional"

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

## Docker-Based Testing Environment

For integration tests requiring external dependencies, we use Docker containers:

```bash
# Start test environment
docker-compose -f docker-compose.test.yml up -d

# Run tests with external dependencies
dotnet test --filter "Category=Integration"

# Stop test environment
docker-compose -f docker-compose.test.yml down
```

## Test Containers

The project also uses Testcontainers for .NET to create isolated Docker containers for testing:

- SQL Server container for database tests
- Redis container for caching tests
- RabbitMQ container for message broker tests

Test containers are automatically created and destroyed during the test run.

## Continuous Integration

The test suite is integrated with our CI/CD pipeline:

1. Unit tests run on every pull request
2. Integration and functional tests run on main branch commits
3. Test coverage reports are generated and published

## Coverage Requirements

- Minimum line coverage: 80%
- Minimum branch coverage: 70%

## Adding New Tests

1. Place tests in the appropriate directory based on test type
2. Follow the existing naming conventions
3. Use the provided test utilities and fixtures
4. Document any special setup or teardown requirements

## Mock Data

The test suite uses:

- AutoFixture for test data generation
- Mock and moq for mocking dependencies
- In-memory databases for data layer testing