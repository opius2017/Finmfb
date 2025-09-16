#!/bin/bash
set -e

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Print with color
print_green() { echo -e "${GREEN}$1${NC}"; }
print_yellow() { echo -e "${YELLOW}$1${NC}"; }
print_red() { echo -e "${RED}$1${NC}"; }

# Check if docker and docker-compose are installed
if ! command -v docker &> /dev/null; then
    print_red "Docker is not installed. Please install Docker and try again."
    exit 1
fi

if ! command -v docker-compose &> /dev/null; then
    print_red "Docker Compose is not installed. Please install Docker Compose and try again."
    exit 1
fi

# Display welcome message
print_green "========================================================"
print_green "  Welcome to FinMFB Banking System Setup"
print_green "========================================================"
print_yellow "This script will set up a local development environment using Docker Compose."
echo ""

# Ask for environment
echo "Select environment to set up:"
select env in "Development" "Production" "Quit"; do
    case $env in
        Development)
            ENV="dev"
            break
            ;;
        Production)
            ENV="prod"
            break
            ;;
        Quit)
            exit 0
            ;;
        *) 
            print_red "Invalid option. Please try again."
            ;;
    esac
done

# Ask if data should be persisted
echo ""
echo "Do you want to persist database data between container restarts?"
select persist in "Yes" "No"; do
    case $persist in
        Yes)
            PERSIST=true
            break
            ;;
        No)
            PERSIST=false
            break
            ;;
        *) 
            print_red "Invalid option. Please try again."
            ;;
    esac
done

# Ask for SQL Server SA password
echo ""
echo "Enter SQL Server SA password (leave empty for default: YourStrongPassword123!):"
read -s SQL_PASSWORD
if [ -z "$SQL_PASSWORD" ]; then
    SQL_PASSWORD="YourStrongPassword123!"
fi

# Create .env file for docker-compose
print_yellow "Creating environment configuration..."
cat > .env << EOF
# Environment
ENVIRONMENT=${ENV}

# SQL Server
ACCEPT_EULA=Y
SA_PASSWORD=${SQL_PASSWORD}
MSSQL_PID=Express

# Redis
REDIS_PASSWORD=RedisStrongPassword123!

# RabbitMQ
RABBITMQ_DEFAULT_USER=admin
RABBITMQ_DEFAULT_PASS=RabbitMQStrongPassword123!

# Grafana
GF_SECURITY_ADMIN_USER=admin
GF_SECURITY_ADMIN_PASSWORD=GrafanaStrongPassword123!

# API URLs
API_URL=http://api
GATEWAY_URL=http://api-gateway

# Ports
SQL_PORT=1433
REDIS_PORT=6379
RABBITMQ_PORT=5672
RABBITMQ_MANAGEMENT_PORT=15672
API_PORT=5000
API_HTTPS_PORT=5001
GATEWAY_PORT=8080
GATEWAY_HTTPS_PORT=8081
FRONTEND_PORT=80
FRONTEND_HTTPS_PORT=443
SEQ_PORT=5341
PROMETHEUS_PORT=9090
GRAFANA_PORT=3000
EOF

print_green "Environment configuration created."

# Start Docker Compose
print_yellow "Starting Docker Compose..."

if [ "$PERSIST" = false ]; then
    print_yellow "Data will NOT be persisted between container restarts."
    docker-compose down -v 2>/dev/null || true
fi

docker-compose up -d

# Check if containers are running
print_yellow "Checking container status..."
CONTAINERS=$(docker-compose ps -q)
if [ -z "$CONTAINERS" ]; then
    print_red "Failed to start containers. Please check the logs with 'docker-compose logs'."
    exit 1
fi

print_green "All containers are running!"

# Display access information
print_green "========================================================"
print_green "  FinMFB Banking System is now running!"
print_green "========================================================"
print_yellow "Access the applications at:"
echo ""
echo "Frontend: http://localhost"
echo "API: http://localhost:5000"
echo "API Swagger: http://localhost:5000/swagger"
echo "API Gateway: http://localhost:8080"
echo "Seq (Logging): http://localhost:5341"
echo "Prometheus: http://localhost:9090"
echo "Grafana: http://localhost:3000"
echo "RabbitMQ Management: http://localhost:15672"
echo ""
print_yellow "Database Connection Information:"
echo "Server: localhost,1433"
echo "User: sa"
echo "Password: ${SQL_PASSWORD}"
echo "Database: FinTechDB"
echo ""
print_yellow "Useful commands:"
echo "- View logs: docker-compose logs -f"
echo "- Stop containers: docker-compose down"
echo "- Restart containers: docker-compose restart"
echo "- Remove all containers and volumes: docker-compose down -v"
echo ""
print_green "Happy coding!"