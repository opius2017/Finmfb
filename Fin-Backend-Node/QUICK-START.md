# Quick Start Guide

Get the FinTech Backend API up and running in minutes!

## Prerequisites

- Node.js >= 20.0.0
- npm >= 10.0.0
- Docker and Docker Compose (recommended)

## Option 1: Docker Compose (Recommended)

The fastest way to get started:

```bash
# 1. Navigate to the project directory
cd Fin-Backend-Node

# 2. Copy environment file
cp .env.example .env

# 3. Start all services (PostgreSQL, Redis, API)
docker-compose up -d

# 4. View logs
docker-compose logs -f api

# 5. Run migrations and seed database
docker-compose exec api npm run migrate
docker-compose exec api npm run db:seed
```

That's it! The API is now running at http://localhost:3000

## Option 2: Manual Setup

If you prefer to run services manually:

```bash
# 1. Navigate to the project directory
cd Fin-Backend-Node

# 2. Run setup script
# For Unix/Linux/Mac:
bash scripts/setup.sh

# For Windows PowerShell:
.\scripts\setup.ps1

# 3. Update .env file with your database and Redis configuration
# Edit .env file

# 4. Start PostgreSQL and Redis
# (Install and start these services on your system)

# 5. Run database migrations
npm run migrate

# 6. Seed database with initial data
npm run db:seed

# 7. Start development server
npm run dev
```

## Verify Installation

### Check Health

```bash
curl http://localhost:3000/health
```

Expected response:
```json
{
  "status": "healthy",
  "timestamp": "2024-01-01T00:00:00.000Z",
  "uptime": 123.456,
  "environment": "development"
}
```

### Test Login

```bash
curl -X POST http://localhost:3000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@fintech.com",
    "password": "Admin@123"
  }'
```

## Access Points

- **API**: http://localhost:3000
- **Health Check**: http://localhost:3000/health
- **Readiness Check**: http://localhost:3000/ready
- **API Docs** (coming soon): http://localhost:3000/api/docs
- **Prisma Studio**: http://localhost:5555 (when running with Docker Compose)

## Default Credentials

After seeding the database:

- **Email**: admin@fintech.com
- **Password**: Admin@123

⚠️ **Important**: Change these credentials in production!

## Common Commands

```bash
# Development
npm run dev              # Start development server with hot reload
npm run build            # Build for production
npm start                # Start production server

# Database
npm run migrate          # Run database migrations
npm run db:seed          # Seed database with initial data
npm run db:studio        # Open Prisma Studio

# Testing
npm test                 # Run tests
npm run test:watch       # Run tests in watch mode
npm run test:coverage    # Run tests with coverage

# Code Quality
npm run lint             # Lint code
npm run lint:fix         # Lint and fix code
npm run format           # Format code with Prettier

# Docker
docker-compose up -d     # Start all services
docker-compose down      # Stop all services
docker-compose logs -f   # View logs
docker-compose ps        # List running services
```

## Troubleshooting

### Port Already in Use

If port 3000 is already in use:

1. Change `PORT` in `.env` file
2. Update `docker-compose.yml` port mapping if using Docker

### Database Connection Error

1. Ensure PostgreSQL is running
2. Check `DATABASE_URL` in `.env` file
3. Verify database credentials

### Redis Connection Error

1. Ensure Redis is running
2. Check `REDIS_HOST` and `REDIS_PORT` in `.env` file

### Migration Errors

```bash
# Reset database (development only)
npm run migrate:reset

# Then run migrations again
npm run migrate
```

## Next Steps

1. ✅ API is running
2. 📚 Read the [README.md](README.md) for detailed documentation
3. 🔧 Explore the [API endpoints](README.md#api-documentation)
4. 🧪 Run tests: `npm test`
5. 🚀 Start building features!

## Need Help?

- Check the [README.md](README.md) for detailed documentation
- Review [PHASE-1-COMPLETE.md](PHASE-1-COMPLETE.md) for implementation details
- Open an issue on GitHub

## What's Next?

The foundation is ready! You can now:

- Implement authentication endpoints
- Add business logic for members, accounts, loans
- Build the calculation engine
- Set up workflow automation
- Integrate with external services

Happy coding! 🚀
