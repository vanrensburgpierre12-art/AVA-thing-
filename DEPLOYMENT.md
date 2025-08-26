# SIM Device Platform - Deployment Guide

This guide covers deploying the complete SIM Device Platform, including the backend API, frontend React application, and infrastructure.

## 🏗️ Architecture Overview

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   React SPA     │    │  .NET Core API  │    │   PostgreSQL    │
│   (Port 3000)   │◄──►│   (Port 5000)   │◄──►│   (Port 5432)   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                │
                       ┌─────────────────┐
                       │     Redis       │
                       │   (Port 6379)   │
                       └─────────────────┘
```

## 🚀 Quick Start (Development)

### Prerequisites

- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 18+** - [Download here](https://nodejs.org/)
- **Docker & Docker Compose** - [Download here](https://www.docker.com/products/docker-desktop)
- **PostgreSQL 15+** (if not using Docker)

### 1. Clone and Setup

```bash
git clone <repository-url>
cd sim-device-platform
```

### 2. Start Infrastructure Services

```bash
cd backend
docker-compose up -d
```

This starts:
- PostgreSQL on port 5432
- Redis on port 6379

### 3. Configure Backend

Update `backend/appsettings.json` with your database connection:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=simdeviceplatform;Username=postgres;Password=password"
  }
}
```

### 4. Run Backend

```bash
cd backend
dotnet restore
dotnet run
```

The API will be available at `http://localhost:5000`

### 5. Run Frontend

```bash
cd frontend
npm install
npm run dev
```

The React app will be available at `http://localhost:3000`

## 🐳 Docker Deployment

### Backend Docker Image

```bash
cd backend
docker build -t simdeviceplatform-api .
docker run -p 5000:5000 simdeviceplatform-api
```

### Frontend Docker Image

```bash
cd frontend
docker build -t simdeviceplatform-frontend .
docker run -p 3000:80 simdeviceplatform-frontend
```

### Complete Stack with Docker Compose

```bash
# From project root
docker-compose -f docker-compose.prod.yml up -d
```

## ☁️ Cloud Deployment

### Azure

#### Backend (.NET Core)

1. **Azure App Service**
   ```bash
   az webapp create --resource-group myResourceGroup --plan myAppServicePlan --name simdeviceplatform-api
   az webapp deployment source config --name simdeviceplatform-api --resource-group myResourceGroup --repo-url <git-repo> --branch main --manual-integration
   ```

2. **Azure Database for PostgreSQL**
   ```bash
   az postgres flexible-server create --resource-group myResourceGroup --name simdeviceplatform-db --admin-user adminuser --admin-password <password>
   ```

3. **Azure Redis Cache**
   ```bash
   az redis create --resource-group myResourceGroup --name simdeviceplatform-redis --sku Basic --vm-size c0
   ```

#### Frontend (React)

1. **Azure Static Web Apps**
   ```bash
   az staticwebapp create --name simdeviceplatform-frontend --resource-group myResourceGroup --source <git-repo> --branch main
   ```

### AWS

#### Backend (.NET Core)

1. **ECS Fargate**
   ```bash
   aws ecs create-cluster --cluster-name simdeviceplatform
   aws ecs create-service --cluster simdeviceplatform --service-name api --task-definition simdeviceplatform-api
   ```

2. **RDS PostgreSQL**
   ```bash
   aws rds create-db-instance --db-instance-identifier simdeviceplatform-db --db-instance-class db.t3.micro --engine postgres
   ```

3. **ElastiCache Redis**
   ```bash
   aws elasticache create-cache-cluster --cache-cluster-id simdeviceplatform-redis --engine redis --cache-node-type cache.t3.micro --num-cache-nodes 1
   ```

#### Frontend (React)

1. **S3 + CloudFront**
   ```bash
   aws s3 mb s3://simdeviceplatform-frontend
   aws s3 website s3://simdeviceplatform-frontend --index-document index.html --error-document index.html
   ```

### Google Cloud

#### Backend (.NET Core)

1. **Cloud Run**
   ```bash
   gcloud run deploy simdeviceplatform-api --image gcr.io/PROJECT_ID/simdeviceplatform-api --platform managed
   ```

2. **Cloud SQL PostgreSQL**
   ```bash
   gcloud sql instances create simdeviceplatform-db --database-version=POSTGRES_15 --tier=db-f1-micro
   ```

3. **Memorystore Redis**
   ```bash
   gcloud redis instances create simdeviceplatform-redis --size=1 --region=us-central1
   ```

#### Frontend (React)

1. **Firebase Hosting**
   ```bash
   firebase init hosting
   firebase deploy
   ```

## 🔧 Configuration

### Environment Variables

#### Backend

```bash
# Database
ConnectionStrings__DefaultConnection=Host=localhost;Database=simdeviceplatform;Username=postgres;Password=password

# JWT
Jwt__Key=your-super-secret-key-with-at-least-32-characters
Jwt__Issuer=https://simdeviceplatform.com
Jwt__Audience=https://simdeviceplatform.com

# Provider API Keys
SimControl__ApiKey=your-simcontrol-api-key
DigitalMatter__ApiKey=your-dm-api-key
Teltonika__ApiKey=your-teltonika-api-key
Flespi__ApiKey=your-flespi-api-key
```

#### Frontend

```bash
# API Configuration
VITE_API_BASE_URL=https://api.simdeviceplatform.com
VITE_AUTH_DOMAIN=your-auth-domain.auth0.com
VITE_AUTH_CLIENT_ID=your-auth-client-id
```

### Database Migrations

```bash
cd backend
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### SSL/TLS Configuration

#### Development
```bash
dotnet dev-certs https --trust
```

#### Production
```bash
# Use proper SSL certificates (Let's Encrypt, etc.)
# Configure reverse proxy (nginx, Apache) for SSL termination
```

## 📊 Monitoring & Observability

### Application Insights (Azure)

```bash
# Add to backend project
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

### CloudWatch (AWS)

```bash
# Add to backend project
dotnet add package Amazon.CloudWatch
```

### Stackdriver (GCP)

```bash
# Add to backend project
dotnet add package Google.Cloud.Diagnostics.AspNetCore
```

### Health Checks

The API includes built-in health checks at `/api/reconcile/health`

## 🔒 Security

### Authentication

1. **JWT Configuration**
   - Use strong, unique keys
   - Set appropriate expiration times
   - Implement refresh token rotation

2. **CORS Configuration**
   ```csharp
   services.AddCors(options =>
   {
       options.AddPolicy("Production", policy =>
       {
           policy.WithOrigins("https://yourdomain.com")
                 .AllowAnyMethod()
                 .AllowAnyHeader();
       });
   });
   ```

### Network Security

1. **Firewall Rules**
   - Restrict database access to application servers only
   - Use VPC/private subnets where possible

2. **API Gateway**
   - Implement rate limiting
   - Add DDoS protection
   - Use WAF for additional security

## 📈 Scaling

### Horizontal Scaling

1. **Load Balancer**
   - Azure Application Gateway
   - AWS Application Load Balancer
   - Google Cloud Load Balancer

2. **Auto-scaling**
   - Azure App Service Plan scaling
   - AWS ECS auto-scaling
   - Google Cloud Run auto-scaling

### Database Scaling

1. **Read Replicas**
   - PostgreSQL read replicas
   - Connection string configuration

2. **Connection Pooling**
   - PgBouncer for PostgreSQL
   - Configure max connections appropriately

## 🚨 Troubleshooting

### Common Issues

1. **Database Connection**
   ```bash
   # Test connection
   psql -h localhost -U postgres -d simdeviceplatform
   ```

2. **API Health Check**
   ```bash
   curl http://localhost:5000/api/reconcile/health
   ```

3. **Frontend Build Issues**
   ```bash
   cd frontend
   rm -rf node_modules package-lock.json
   npm install
   ```

### Logs

1. **Backend Logs**
   ```bash
   # Development
   dotnet run --environment Development
   
   # Production
   journalctl -u simdeviceplatform-api -f
   ```

2. **Frontend Logs**
   ```bash
   # Check browser console
   # Check network tab for API calls
   ```

## 📚 Additional Resources

- [.NET Core Deployment Guide](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/)
- [React Deployment Guide](https://create-react-app.dev/docs/deployment/)
- [PostgreSQL Best Practices](https://www.postgresql.org/docs/current/admin.html)
- [Redis Configuration](https://redis.io/topics/config)

## 🆘 Support

For deployment issues:
1. Check the logs
2. Verify configuration
3. Test connectivity between services
4. Review security group/firewall rules
5. Check resource limits and quotas