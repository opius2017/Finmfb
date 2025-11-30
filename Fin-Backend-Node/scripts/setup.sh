#!/bin/bash

# Setup script for FinTech Backend API

set -e

echo "🚀 Setting up FinTech Backend API..."

# Check Node.js version
echo "📦 Checking Node.js version..."
NODE_VERSION=$(node -v | cut -d'v' -f2 | cut -d'.' -f1)
if [ "$NODE_VERSION" -lt 20 ]; then
  echo "❌ Node.js version 20 or higher is required"
  exit 1
fi
echo "✅ Node.js version: $(node -v)"

# Check npm version
echo "📦 Checking npm version..."
NPM_VERSION=$(npm -v | cut -d'.' -f1)
if [ "$NPM_VERSION" -lt 10 ]; then
  echo "❌ npm version 10 or higher is required"
  exit 1
fi
echo "✅ npm version: $(npm -v)"

# Install dependencies
echo "📦 Installing dependencies..."
npm install

# Copy environment file if it doesn't exist
if [ ! -f .env ]; then
  echo "📝 Creating .env file from .env.example..."
  cp .env.example .env
  echo "⚠️  Please update .env file with your configuration"
fi

# Setup Husky
echo "🐶 Setting up Husky..."
npm run prepare

# Generate Prisma client
echo "🔧 Generating Prisma client..."
npx prisma generate

# Create logs directory
echo "📁 Creating logs directory..."
mkdir -p logs

echo ""
echo "✅ Setup completed successfully!"
echo ""
echo "Next steps:"
echo "1. Update .env file with your configuration"
echo "2. Start PostgreSQL and Redis (or use docker-compose up -d)"
echo "3. Run database migrations: npm run migrate"
echo "4. Seed database: npm run db:seed"
echo "5. Start development server: npm run dev"
echo ""
echo "For Docker setup:"
echo "  docker-compose up -d"
echo ""
