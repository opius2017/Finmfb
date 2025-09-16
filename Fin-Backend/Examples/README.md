# Accounting Module Integration Workflow Examples

This document describes the sample workflow examples that demonstrate the integration between operational modules and the accounting engine.

## Overview

The examples showcase end-to-end workflows that demonstrate how domain events from operational modules are translated into accounting journal entries. Each workflow covers a typical business process in a specific module.

## Available Workflows

### 1. Banking Transaction Workflow

This workflow demonstrates banking operations and their accounting impact:

- Creating a bank account
- Processing deposits
- Processing withdrawals
- Transferring funds between accounts
- Charging fees
- Paying interest

**Endpoint:** `POST /api/workflowexamples/banking`

### 2. Loan Processing Workflow

This workflow demonstrates the loan lifecycle and its accounting impact:

- Creating a loan
- Charging processing fees
- Disbursing loan funds
- Accruing interest
- Processing repayments
- Writing off a loan

**Endpoint:** `POST /api/workflowexamples/loan`

### 3. Payroll Processing Workflow

This workflow demonstrates the payroll cycle and its accounting impact:

- Creating a payroll period
- Adding employee salary payments
- Processing payroll
- Accruing expenses
- Remitting taxes
- Remitting pension contributions
- Processing bonus payments

**Endpoint:** `POST /api/workflowexamples/payroll`

### 4. Fixed Asset Lifecycle Workflow

This workflow demonstrates the fixed asset lifecycle and its accounting impact:

- Creating asset categories
- Acquiring new assets
- Recording monthly depreciation
- Revaluing assets
- Recording impairment
- Disposing of assets

**Endpoint:** `POST /api/workflowexamples/fixed-asset`

## Technical Details

Each workflow example demonstrates:

1. **Domain Entities**: Rich domain models with business logic
2. **Domain Events**: Events raised by domain entities during business operations
3. **Event Handlers**: Components that process domain events
4. **Integration Services**: Services that translate domain events into accounting entries
5. **Journal Entries**: Double-entry accounting records created in the accounting system

## Event-Driven Architecture

The integration between operational modules and the accounting engine is implemented using an event-driven architecture:

1. Domain entities raise domain events during business operations
2. The `DomainEventService` processes these events during the `SaveChangesAsync` operation
3. Event handlers route events to the appropriate integration services
4. Integration services create balanced journal entries in the accounting system

This approach maintains loose coupling between modules while ensuring all business operations are properly recorded in the accounting system.