#!/bin/bash

# Update all DTOs in the GeneralLedger folder to use the correct namespace
find /workspaces/Finmfb/Fin-Backend/Application/DTOs/GeneralLedger -name "*.cs" -type f -exec sed -i 's/namespace FinTech\.Application\.DTOs/namespace FinTech.Core.Application.DTOs/g' {} \;

echo "Namespace updates completed!"