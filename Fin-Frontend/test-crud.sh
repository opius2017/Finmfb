#!/bin/bash

###############################################################################
# Frontend CRUD Testing Script
# Tests all Create, Read, Update, Delete operations across dashboards
###############################################################################

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
BASE_URL="${BASE_URL:-http://localhost:5173}"
API_URL="${API_URL:-http://localhost:5000}"
BROWSER="${BROWSER:-chromium}"

echo -e "${BLUE}╔════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║     Frontend CRUD Operations Testing Suite                ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════════╝${NC}"
echo ""

# Check if frontend is running
echo -e "${YELLOW}→ Checking if frontend is running...${NC}"
if curl -s "${BASE_URL}" > /dev/null; then
    echo -e "${GREEN}✓ Frontend is running at ${BASE_URL}${NC}"
else
    echo -e "${RED}✗ Frontend is not running at ${BASE_URL}${NC}"
    echo -e "${YELLOW}  Starting frontend server...${NC}"
    npm run dev &
    FRONTEND_PID=$!
    sleep 10
fi

# Check if backend is running
echo -e "${YELLOW}→ Checking if backend is running...${NC}"
if curl -s "${API_URL}/health" > /dev/null 2>&1; then
    echo -e "${GREEN}✓ Backend is running at ${API_URL}${NC}"
else
    echo -e "${YELLOW}⚠ Backend might not be running at ${API_URL}${NC}"
    echo -e "${YELLOW}  Some tests may fail without backend${NC}"
fi

echo ""
echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}  Running E2E Tests${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo ""

# Install Playwright browsers if not already installed
if [ ! -d "node_modules/@playwright/test" ]; then
    echo -e "${YELLOW}→ Installing Playwright...${NC}"
    npm install --save-dev @playwright/test
fi

echo -e "${YELLOW}→ Installing Playwright browsers...${NC}"
npx playwright install --with-deps ${BROWSER}

# Run tests
echo ""
echo -e "${GREEN}→ Running CRUD operation tests...${NC}"
echo ""

# Run all tests
npx playwright test --project=${BROWSER} --reporter=list,html

TEST_EXIT_CODE=$?

echo ""
echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}  Test Results${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo ""

if [ $TEST_EXIT_CODE -eq 0 ]; then
    echo -e "${GREEN}✓ All CRUD tests passed successfully!${NC}"
    echo ""
    echo -e "${GREEN}Test Coverage:${NC}"
    echo -e "  ✓ Customer Management (Create, Read, Update, Delete)"
    echo -e "  ✓ Loan Management (Create, Read, Update, Repayment)"
    echo -e "  ✓ Inventory Management (Create, Read, Update, Delete)"
    echo -e "  ✓ Accounts Payable (Create, Read, Payment)"
    echo -e "  ✓ Accounts Receivable (Create, Read, Payment)"
    echo -e "  ✓ Payroll Management (Create, Read, Process)"
    echo -e "  ✓ Dashboard Navigation"
    echo -e "  ✓ Search and Filter"
    echo -e "  ✓ Form Validation"
else
    echo -e "${RED}✗ Some tests failed${NC}"
    echo -e "${YELLOW}  Check the HTML report for details:${NC}"
    echo -e "  ${BLUE}npx playwright show-report${NC}"
fi

echo ""
echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}  Additional Commands${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo ""
echo -e "View HTML Report:    ${GREEN}npx playwright show-report${NC}"
echo -e "Run specific test:   ${GREEN}npx playwright test --grep \"Customer\"${NC}"
echo -e "Debug mode:          ${GREEN}npx playwright test --debug${NC}"
echo -e "UI mode:             ${GREEN}npx playwright test --ui${NC}"
echo -e "Run in headed mode:  ${GREEN}npx playwright test --headed${NC}"
echo ""

# Cleanup
if [ ! -z "$FRONTEND_PID" ]; then
    echo -e "${YELLOW}→ Stopping frontend server...${NC}"
    kill $FRONTEND_PID 2>/dev/null || true
fi

exit $TEST_EXIT_CODE
