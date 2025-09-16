#!/bin/bash

# Exit on error
set -e

# Display help
if [ "$1" == "-h" ] || [ "$1" == "--help" ]; then
    echo "Usage: $0 [option]"
    echo "Options:"
    echo "  --unit         Run only unit tests"
    echo "  --integration  Run only integration tests"
    echo "  --functional   Run only functional tests"
    echo "  --all          Run all tests (default)"
    echo "  --coverage     Generate test coverage report"
    echo "  --help, -h     Display this help message"
    exit 0
fi

# Set default parameters
TEST_FILTER=""
GENERATE_COVERAGE=false

# Parse command-line arguments
for arg in "$@"; do
    case $arg in
        --unit)
            TEST_FILTER="Category=Unit"
            ;;
        --integration)
            TEST_FILTER="Category=Integration"
            ;;
        --functional)
            TEST_FILTER="Category=Functional"
            ;;
        --all)
            TEST_FILTER=""
            ;;
        --coverage)
            GENERATE_COVERAGE=true
            ;;
    esac
done

# Set colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[0;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Building solution...${NC}"
dotnet build

# Run tests based on the filter
run_tests() {
    local filter=$1
    local command="dotnet test"
    
    if [ -n "$filter" ]; then
        command="$command --filter \"$filter\""
    fi
    
    if [ "$GENERATE_COVERAGE" = true ]; then
        command="$command /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura"
    fi
    
    echo -e "${YELLOW}Running tests: $command${NC}"
    eval $command
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}Tests completed successfully!${NC}"
    else
        echo -e "${RED}Tests failed!${NC}"
        exit 1
    fi
}

# Start Docker containers for integration and functional tests
if [[ -z "$TEST_FILTER" || "$TEST_FILTER" == *"Integration"* || "$TEST_FILTER" == *"Functional"* ]]; then
    echo -e "${YELLOW}Starting Docker test environment...${NC}"
    docker-compose -f Fin-Backend.Tests/docker-compose.test.yml up -d
fi

# Run the tests
run_tests "$TEST_FILTER"

# Generate coverage report if requested
if [ "$GENERATE_COVERAGE" = true ]; then
    echo -e "${YELLOW}Generating test coverage report...${NC}"
    
    # Check if reportgenerator is installed
    if ! command -v reportgenerator &> /dev/null; then
        echo -e "${YELLOW}Installing reportgenerator...${NC}"
        dotnet tool install -g dotnet-reportgenerator-globaltool
    fi
    
    reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
    
    echo -e "${GREEN}Coverage report generated in coveragereport directory${NC}"
    
    # Open the report if on a desktop environment
    if command -v xdg-open &> /dev/null; then
        xdg-open coveragereport/index.html
    elif command -v open &> /dev/null; then
        open coveragereport/index.html
    fi
fi

# Stop Docker containers if they were started
if [[ -z "$TEST_FILTER" || "$TEST_FILTER" == *"Integration"* || "$TEST_FILTER" == *"Functional"* ]]; then
    echo -e "${YELLOW}Stopping Docker test environment...${NC}"
    docker-compose -f Fin-Backend.Tests/docker-compose.test.yml down
fi

echo -e "${GREEN}All done!${NC}"