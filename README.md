# SIM and Device Management Platform

A unified platform that consolidates SIMs and devices from **SimControl**, **Digital Matter (DME)**, **Teltonika FOTA Web**, and **flespi** into one authoritative system.

## 🎯 Goals

- **Authoritatively maps** ICCID ⇄ device (IMEI/serial) ⇄ asset
- **Surfaces a single source of truth** and reconciliation health
- **Lets ops act** (tag SIMs, push FOTA, enable/disable, assign assets)
- **Exports reports** and keeps a historical audit trail
- **Front-end:** React (SPA) dashboard for devices/SIMs/assets/health/reports

## 🏗️ Architecture

```
React Frontend (SPA) → API Gateway/BFF → Data Processing Service → Provider Adapters
                                                              ↓
                                              Job Runner + Event Bus → Integrations
```

## 🚀 Quick Start

### Backend
```bash
cd backend
docker-compose up -d
npm install
npm run dev
```

### Frontend
```bash
cd frontend
npm install
npm run dev
```

## 📁 Project Structure

- `backend/` - .NET Core API with GraphQL, data processing, and provider adapters
- `frontend/` - React SPA with TypeScript, shadcn/ui, and TanStack Query
- `infra/` - Terraform configurations and deployment scripts
- `docs/` - API documentation and architecture details

## 🔧 Key Features

- **Unified Device View** - Single table showing all devices with SIM and asset linkages
- **Smart Reconciliation** - Automatic matching with confidence scores
- **SIM Tagging Pipeline** - Automated description updates with audit trail
- **FOTA Management** - Teltonika firmware scheduling and monitoring
- **Comprehensive Reporting** - CSV exports for all reconciliation scenarios
- **Health Monitoring** - Provider sync status and system health KPIs

## 📊 Data Model

Core entities: `sims`, `devices`, `assets`, `links_device_sim`, `links_asset_device`, `audit_events`, `fota_jobs`

## 🔐 Security

- OIDC/JWT authentication
- RBAC: viewer, ops, admin
- Encrypted at rest and in transit
- Comprehensive audit logging

## 📈 Success Metrics

- % devices in Active Linked vs total
- Time-to-consistency after provider change
- Duplicate ICCID count trending down
- Mean time to tag/update SIM descriptions