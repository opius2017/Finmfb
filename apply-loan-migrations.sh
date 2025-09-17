#!/bin/bash

# Script to apply the Loan Management System database migrations

# Set the working directory to the project root
cd "$(dirname "$0")"

echo "========================================================"
echo "     Applying Loan Management System Migrations"
echo "========================================================"

# Check if dotnet-ef is installed
if ! command -v dotnet-ef &> /dev/null; then
    echo "Installing Entity Framework Core tools..."
    dotnet tool install --global dotnet-ef
    
    # Check if installation was successful
    if [ $? -ne 0 ]; then
        echo "Failed to install Entity Framework Core tools. Please install manually."
        exit 1
    fi
fi

# Apply the migrations
echo "Applying database migrations..."
dotnet ef database update --project Fin-Backend/FinTech.WebAPI.csproj --context ApplicationDbContext

# Check if migration was successful
if [ $? -ne 0 ]; then
    echo "Migration failed. Please check the error messages above."
    exit 1
else
    echo "Migration completed successfully!"
fi

echo "========================================================"
echo "   Loan Management System Migration Complete"
echo "========================================================"

# List the applied migrations
echo "Listing applied migrations:"
dotnet ef migrations list --project Fin-Backend/FinTech.WebAPI.csproj --context ApplicationDbContext

echo "For more information, see Fin-Backend/LOAN-MANAGEMENT-MIGRATION.md"

exit 0