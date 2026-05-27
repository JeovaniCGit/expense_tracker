# Expense Tracker

A modern expense tracking API built with .NET 8, ASP.NET Core, PostgreSQL, Entity Framework Core, and Clean Architecture principles. The application helps users organize expenses into categories and collections, while providing secure authentication, role/permission-based authorization, background processing, structured API errors, and integration test coverage.

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Project Architecture](#project-architecture)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
  - [Local Development Setup](#local-development-setup)
  - [Configuration](#configuration)
  - [Database Setup](#database-setup)
  - [Running the Application](#running-the-application)
- [Docker Deployment](#docker-deployment)
- [API Documentation](#api-documentation)
- [Concurrency Handling](#concurrency-handling)
- [Error Responses](#error-responses)
- [Testing](#testing)
- [CI](#ci)
- [Future Enhancements](#future-enhancements)
- [Contributing](#contributing)
- [License](#license)

## Overview

Expense Tracker is a personal finance Web API designed to help users track expenses, organize them by category, and group related expenses into collections. The project follows a feature-oriented structure while preserving Clean Architecture boundaries between API, Application, Domain, and Infrastructure layers.

Recent updates introduced improved API structure, `/accounts/me` routes for user-owned resources, integration test infrastructure, JWT configuration improvements, standardized error responses, optimistic concurrency handling, and GitHub Actions CI.

## Features

### Current Features

- **Authentication & Authorization**
  - JWT-based authentication
  - Separate access and refresh token signing configuration
  - Secure refresh/login flows
  - Role-based access control
  - Permission-based authorization
  - Authentication cookies for token handling

- **Account Management**
  - User registration through the authentication flow
  - User profile retrieval
  - User update and delete operations
  - Admin account listing and analytics endpoints

- **Expense Records**
  - Create, read, update, and delete transaction records
  - Records are associated with both categories and collections
  - Query records by category
  - Query records by collection

- **Category Management**
  - Create, list, update, bulk update, and delete user categories
  - User-owned category routes use `/accounts/me`

- **Collection Management**
  - Create, list, update, and delete user collections
  - Group related records into expense collections
  - Store estimated and real budget data

- **Email Services**
  - Email verification flow
  - Password reset flow
  - SendGrid integration
  - Email delivery tracking

- **Background Jobs**
  - Scheduled recurring tasks via Hangfire
  - Job persistence and monitoring
  - Dashboard for job management
  - Hangfire disabled in test environment

- **API Reliability**
  - API versioning
  - Request timeout policies
  - Rate limiting
  - CORS configuration
  - Swagger/OpenAPI documentation
  - Structured API error responses
  - Optimistic concurrency protection through **PostgreSQL** `xmin` property and **EF Core** row versoning

- **Testing**
  - Unit tests
  - Integration tests using Testcontainers PostgreSQL
  - WireMock-based external service stubbing
  - Test authentication handler for protected endpoint coverage
  - Concurrency tests for users, categories, collections, and records

- **CI**
  - GitHub Actions workflow for restore, build, and test on .NET 8

### Planned Features

- 🤖 **AI-Powered Analysis** - Spending pattern insights and financial recommendations
- 📦 **Full API Containerization** - Production-ready API container deployment
- ☁️ **Azure Deployment** - Cloud hosting and deployment pipeline support
- 🗄️ **Azure PostgreSQL** - Managed PostgreSQL database hosting
- 🔐 **Azure Key Vault** - Centralized secret management
- 🧑‍💼 **Azure Entra ID** - Future authentication provider integration

## Technology Stack

### Backend

- **.NET 8**
- **ASP.NET Core Web API**
- **Entity Framework Core 9.0**

### Database

- **PostgreSQL**
- **EF Core Migrations**
- **PostgreSQL row versioning / xmin-based concurrency**

### Authentication & Security

- **JWT Bearer Authentication**
- **BCrypt.Net**
- **Permission-based authorization policies**
- **Secure authentication cookie factory**

### Validation & Error Handling

- **FluentValidation**
- **ErrorOr**
- Standardized `ApiErrorResponse` payloads

### External Services

- **SendGrid**
- **FluentEmail**
- **WireMock.Net** for integration test stubs

### Infrastructure & DevOps

- **Hangfire**
- **Serilog**
- **Docker**
- **GitHub Actions**
- **Testcontainers**

### Testing

- **xUnit**
- **FluentAssertions**
- **Moq**
- **Microsoft.AspNetCore.Mvc.Testing**
- **Testcontainers.PostgreSql**
- **WireMock.Net**

## Project Architecture

The repository follows a **feature-based folder structure** while preserving Clean Architecture responsibilities.
