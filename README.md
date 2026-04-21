# Expense Tracker

A modern, full-featured expense tracking application designed to help users keep a visual track of their expenses with future AI-driven financial analysis capabilities.

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Project Architecture](#project-architecture)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
  - [Local Development Setup](#local-development-setup)
  - [Environment Configuration](#environment-configuration)
  - [Database Setup](#database-setup)
  - [Running the Application](#running-the-application)
- [Docker Deployment](#docker-deployment)
- [API Documentation](#api-documentation)
- [Project Structure](#project-structure)
- [Testing](#testing)
- [Future Enhancements](#future-enhancements)
- [Contributing](#contributing)

## Overview

Expense Tracker is a personal finance application built with modern cloud-native technologies. It provides a clean, intuitive interface for tracking expenses and organizing them into categories and collections. The application is designed with scalability and future extensibility in mind, with planned AI integration for comprehensive financial analysis.

## Features

### Current Features

- **User Authentication & Authorization**
  - Secure JWT-based authentication
  - Role-based access control (RBAC)
  - Permission-based authorization
  - Account management and security

- **Expense Management**
  - Create, read, update, and delete expense records
  - Organize expenses by categories
  - Group related expenses into collections
  - Visual expense tracking

- **Category Management**
  - Create and manage custom expense categories
  - Assign expenses to specific categories

- **Email Services**
  - Email verification for new accounts
  - Password reset functionality
  - Integrated with SendGrid for reliable email delivery

- **Background Jobs**
  - Scheduled recurring tasks via Hangfire
  - Job persistence and monitoring
  - Dashboard for job management

- **API Features**
  - RESTful API with comprehensive error handling
  - API versioning for backward compatibility
  - Rate limiting for abuse prevention
  - CORS enabled for frontend integration
  - Swagger/OpenAPI documentation
  - Request timeout handling

- **Logging & Monitoring**
  - Structured logging with Serilog
  - Console and file-based log outputs
  - Request context tracking
  - Daily log file rotation

### Planned Features

- 🤖 **AI-Powered Analysis** - Machine learning-based insights for spending patterns and financial recommendations

## Technology Stack

### Backend
- **.NET 8** - Modern cross-platform .NET framework
- **ASP.NET Core Web API** - High-performance web framework
- **Entity Framework Core 9.0** - Object-relational mapping (ORM)

### Database
- **PostgreSQL 16** - Open-source relational database
- **Entity Framework Core Migrations** - Schema versioning and management

### Authentication & Security
- **JWT Bearer Tokens** - Stateless authentication
- **BCrypt.Net** - Secure password hashing
- **System.IdentityModel.Tokens.Jwt** - JWT token validation

### External Services
- **SendGrid** - Email delivery service
- **FluentEmail** - Email template engine

### Infrastructure & DevOps
- **Hangfire** - Background job processing and scheduling
- **Serilog** - Structured logging framework
- **Docker & Docker Compose** - Containerization

### API & Documentation
- **Swagger/Swashbuckle** - OpenAPI documentation
- **ASP Versioning** - API version management

### Additional Libraries
- **FluentValidation** - Request validation
- **ErrorOr** - Result-based error handling
- **DotNetEnv** - Environment variable management
- **Quartz.NET** - Advanced job scheduling capabilities

## Project Architecture

This repository follows a **feature-based folder structure** while still applying Clean Architecture principles. Each feature keeps related API, application, domain, and infrastructure code together so you can work end-to-end without jumping across unrelated folders.

```
ExpenseTracker/
├── src/
│   ├── Api/                     # Web API layer
│   │   └── Features/            # Feature-based organization inside API
│   │       ├── Accounts/        # Controllers: AccountsController, AdminAccountsController
│   │       ├── Authentication/  # Controllers, Auth cookies
│   │       ├── Categories/      # Controllers
│   │       ├── Collections/     # Controllers
│   │       └── Records/         # Controllers
│   │
│   ├── Application/             # Business logic, services, DTOs, validators
│   │   └── Features/            # Feature-based organization inside Application layer
│   │       ├── Accounts/        # UserService, AdminServices, DTOs, Validators
│   │       ├── Authentication/  # AuthenticationService, JWT handling, DTOs, Validators
│   │       ├── Categories/      # TransactionRecordCategoryService, DTOs, Validators
│   │       ├── Collections/     # CollectionService, DTOs, Validators
│   │       └── Records/         # TransactionRecordService, DTOs, Validators
│   │
│   ├── Domain/                  # Entities, value objects, repository interfaces
│   │   └── Features/            # Feature-based organization inside Domain layer
│   │       ├── Accounts/        # User, PasswordHistory, IUserRepository
│   │       ├── Authorization/   # Permissions, Tokens, UserRoles
│   │       ├── Categories/      # TransactionRecordCategory, ITransactionRecordCategoryRepository
│   │       ├── Collections/     # TransactionCollection, ITransactionCollectionRepository
│   │       └── Records/         # TransactionRecord, ITransactionRecordRepository
│   │
│   └── Infrastructure/          # EF Core, repositories, external services, background jobs
│       └── Features/            # Feature-based organization inside Infrastructure
│           ├── Accounts/        # UserRepository, AdminAnalyticsService, CurrentUserService
│           ├── Database/        # DbContext, Migrations
│           ├── Emails/          # SendGrid integration, templating
│           ├── Hangfire/        # Background jobs
│           └── Records/         # Record persistence
│
└── tests/                       # Unit and integration tests, organized per feature
    ├── UnitTests/
    └── IntegrationTests/
```

### Why this structure?

We follow a feature-based folder structure to keep related code together and reduce cognitive overhead. Each feature encapsulates its API endpoints, application logic, domain models, and infrastructure support, while still preserving a clean separation of responsibilities.

### Layer Responsibilities

- **API Layer**: HTTP request handling, routing, and feature controllers
- **Application Layer**: Use case orchestration, validation, permissions, and feature services
- **Domain Layer**: Core entities, value objects, and domain behavior
- **Infrastructure Layer**: Persistence, external services, and background job configuration

## Prerequisites

- **.NET 8** - [Download from Microsoft](https://dotnet.microsoft.com/download)
- **Docker & Docker Compose** - [Download from Docker](https://www.docker.com/products/docker-desktop)
- **PostgreSQL 16** (optional, if running locally without Docker)
- **Git** - For version control

## Getting Started

### Local Development Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd expense_tracker
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Install .NET tools** (if needed)
   ```bash
   dotnet tool restore
   ```

### Environment Configuration

1. **Create or update the `.env` file** in `src/Api/ExpenseTrackerWebAPI/`

   ```env
   # Database Configuration
   EXPENSETRACKER_CONNECTION_STRING=your_connection_string;
   POSTGRES_USER=your_username
   POSTGRES_PASSWORD=your_password
   POSTGRES_DB=your_db_name
   POSTGRES_PORT=some_port

   # JWT Configuration
   JWT_SIGNINGKEY=your-secure-256-bit-base64-encoded-secret-key
   JWT_ISSUER=your_token_issuer
   JWT_AUDIENCE=your_token_audience
   JWT_ACCESSTOKEN_EXPIRYMINUTES=access_token_expiry_in_minutes
   JWT_REFRESHTOKEN_EXPIRYDAYS=refresh_token_expiry_in_days

   # Email Configuration
   FROM_EMAIL=your-email@example.com
   FROM_NAME=YourAppName
   SEND_GRID_API_KEY=your-sendgrid-api-key
   SEND_GRID_VERIFICATION_TEMPLATE_ID=your-template-id
   SEND_GRID_RESET_TEMPLATE_ID=your-template-id

   # CORS Configuration
   CORS_ALLOWEDORIGINS=your_allowed_origins
   ```

**Important**: Never commit `.env` files with real secrets. Use secure secret management in production.

### Database Setup

#### Option 1: Using Docker Compose (Recommended)

```bash
cd src/Api/ExpenseTrackerWebAPI
docker-compose up -d
```

This will start a PostgreSQL 16 container with the configuration from your `.env` file.

#### Option 2: Using Local PostgreSQL

Ensure PostgreSQL 16 is installed and running, then update your connection string in `.env`.

#### Apply Migrations

Once the database is running:

```bash
cd src/Infrastructure/ExpenseTrackerInfrastructure
dotnet ef database update --startup-project ../../Api/ExpenseTrackerWebAPI/ExpenseTracker.API.csproj
```

### Running the Application

1. **From the root directory**, start the API:

   ```bash
   cd src/Api/ExpenseTrackerWebAPI
   dotnet run
   ```

   The API will start at `https://localhost:5001` (HTTPS) or `http://localhost:5000` (HTTP).

2. **Access Swagger UI**
   
   Open your browser and navigate to:
   ```
   http://localhost:5000/swagger
   ```

3. **Access Hangfire Dashboard** (for background jobs)
   
   Navigate to:
   ```
   http://localhost:5000/hangfire
   ```

## Docker Deployment

The repository includes a `docker-compose.yml` file for local development support with PostgreSQL.

```bash
cd src/Api/ExpenseTrackerWebAPI
docker-compose up -d
```

This starts a PostgreSQL service using the local environment variables defined in `.env`.

> Full application containerization and Azure deployment will be added in a future iteration.

## API Documentation

### Swagger/OpenAPI

The API documentation is automatically generated and available at:
- **Development**: `http://localhost:5000/swagger`
- **Production**: `/swagger` (when enabled)

### API Endpoints

The API is organized into the following controller groups:

- **Accounts** - User account management
  - `POST /api/accounts/register` - Register new account
  - `POST /api/accounts/login` - Authenticate user
  - `GET /api/accounts/profile` - Get user profile
  
- **Categories** - Expense category management
  - `GET /api/categories` - List all categories
  - `POST /api/categories` - Create category
  - `PUT /api/categories/{id}` - Update category
  - `DELETE /api/categories/{id}` - Delete category

- **Collections** - Expense collection management
  - `GET /api/collections` - List all collections
  - `POST /api/collections` - Create collection
  - `PUT /api/collections/{id}` - Update collection
  - `DELETE /api/collections/{id}` - Delete collection

- **Records** - Expense record management
  - `GET /api/records` - List all expense records
  - `POST /api/records` - Create expense record
  - `PUT /api/records/{id}` - Update expense record
  - `DELETE /api/records/{id}` - Delete expense record

### API Versioning

The API supports multiple versions through ASP.NET versioning:

```bash
# Access version 1.0
GET /api/v1.0/records

# Access version 2.0
GET /api/v2.0/records
```

### Authentication

Most endpoints require JWT authentication. Include the token in the Authorization header:

```bash
curl -H "Authorization: Bearer <your-jwt-token>"
```

## Project Structure

### Domain Layer (`src/Domain`)
Contains core business entities and domain logic. This layer has no dependencies on external frameworks.

- **Entities**: User, Account, Record, Category, Collection
- **Value Objects**: Money, DateRange
- **Domain Services**: Business rule implementations

### Application Layer (`src/Application`)
Implements feature use cases and orchestrates domain objects. Contains service interfaces, contracts, and permission seeding.

- **Accounts**: Account flows and user management
- **Authentication**: Login, token handling, and refresh flows
- **Authorization**: Permission management and role definitions
  - `Perms/Seeds/` — `PermissionSeeds` contains fixed permission entries seeded into the database
  - `UserRoles/Enums/` — `UserRoleEnum` defines fixed user roles seeded in the database
- **Categories**: Category creation and management
- **Collections**: Expense collection flows
- **Emails**: SendGrid-based email verification and reset
- **Records**: Expense record handling and reporting
- **Abstractions**: Cross-feature interfaces and shared contracts

### Infrastructure Layer (`src/Infrastructure`)
Handles persistence, external service adapters, and database support.

- **Accounts**: Account persistence and identity support
- **Authentication**: JWT and token storage integration
- **Authorization**: Role and permission persistence
- **Categories**: Category persistence
- **Collections**: Collection persistence
- **Database**: EF Core DbContext and database configuration
- **Emails**: SendGrid and email templating integration
- **Migrations**: Database schema migration files

### API Layer (`src/Api`)
Presents the application to clients through HTTP endpoints.

- **Controllers**: Feature-based HTTP endpoint implementations
- **Middleware**: Custom request/response processing
- **Swagger**: OpenAPI documentation configuration
- **Logging**: Request context and error handling
- **Validation**: Request validation middleware

## Testing

### Unit Tests

```bash
cd tests/UnitTests/ExpenseTrackerApplicationTests
dotnet test
```

### Integration Tests

```bash
cd tests/IntegrationTests
dotnet test
```

### Running All Tests

From the root directory:

```bash
dotnet test
```

## Future Enhancements

- 🤖 **AI-Powered Analysis** - Machine learning integration for spending pattern analysis and financial recommendations
- 📦 **API Containerization** - Dockerize the full API for production-ready deployment
- ☁️ **Azure Store Hosting** - Deploy the application through Azure deployment pipelines
- 🗄️ **Azure PostgreSQL Migration** - Move the database to Azure Database for PostgreSQL
- 🔐 **Azure Key Vault** - Secure secrets and configuration using Azure Key Vault
- 🧑‍💼 **Azure Entra ID** - Migrate authentication to Azure Entra ID / Azure AD
- ✔️ **Integration Tests** - Add integration test coverage for API workflows

## Contributing

This is a personal project, but contributions and suggestions are welcome. 

## License

This project is unlicensed. All rights reserved.

---

## Support

For issues, questions, or suggestions, please create an issue in the repository.

## Acknowledgments

- Built with [.NET 8](https://dotnet.microsoft.com/)
- Database by [PostgreSQL](https://www.postgresql.org/)
- Email services via [SendGrid](https://sendgrid.com/)
- Logging powered by [Serilog](https://serilog.net/)
- Background jobs with [Hangfire](https://www.hangfire.io/)
