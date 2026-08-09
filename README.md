# PaymentSimplifier

PaymentSimplifier is a simple project that simulates a transaction environment between users. It exposes a small HTTP API to create users, add balance, and transfer money while applying a few business rules commonly found in payment systems.

## Overview

The application was built to model a simplified payment flow:

- create common users and merchant users
- deposit money into an account
- transfer money from one user to another
- validate the transfer with an external authorization service
- notify the payee after a successful transfer

This project is intentionally small and focused, making it useful for learning, testing, and extending payment-related business logic.

## Features

- User registration with document (CPF/CNPJ) and email uniqueness
- Support for common users and merchant users 
- Balance deposit endpoint
- Transfer endpoint with business validations
- Transaction persistence in PostgreSQL
- Automatic EF Core migrations on startup
- Docker and Docker Compose support
- Postman collection for quick API testing

## Business Rules

- Only users of type `Commom` can send money
- Merchant users can receive money
- The payer and payee cannot be the same user
- Transfer amount must be greater than zero
- The payer must have enough balance
- A transfer depends on external authorization before completion
- If payee notification fails, the transfer is still completed

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Docker
- Docker Compose

## How To Run Locally

### Prerequisites

- .NET 10 SDK
- PostgreSQL

### 1. Configure the database connection

The application expects a PostgreSQL connection string in `ConnectionStrings:Postgres`.

### 2. Restore dependencies

```powershell
dotnet restore
```

### 3. Run the API

```powershell
dotnet run --project PaymentSimplifier
```

By default, the local development profile runs on:

```text
http://localhost:5049
```

The application applies database migrations automatically during startup.

### 4. Optional: OpenAPI document

In development mode, the API exposes OpenAPI metadata at:

```text
http://localhost:5049/openapi/v1.json
```

## How To Run With Docker Compose

The project already includes a `.env`, `Dockerfile`, and `docker-compose.yml`.

### 1. Start the containers

```powershell
docker compose up --build
```

### 2. Access the API

With the current `.env`, the API will be available at:

```text
http://localhost:8080
```

The PostgreSQL container will be exposed at:

```text
localhost:5432
```

### 3. Stop the containers

```powershell
docker compose down
```

To also remove the persisted database volume:

```powershell
docker compose down -v
```

## API Endpoints

Base URL when running locally:

```text
http://localhost:5049
```

Base URL when running with Docker Compose:

```text
http://localhost:8080
```

### `POST /Users`

Creates a new user.

Request body:

```json
{
  "name": "Lucas Silva",
  "document": "529.982.247-25",
  "email": "lucas@email.com",
  "password": "123456",
  "userType": 1
}
```

`userType` values:

- `1` = `Commom`
- `2` = `Merchant`

Example response:

```json
{
  "id": "7d7d7d7d-1111-2222-3333-444444444444",
  "name": "Lucas Silva",
  "document": "52998224725",
  "email": "lucas@email.com",
  "userType": 1,
  "balance": 0
}
```

### `PATCH /Users/{userId}/deposit`

Deposits money into a user's account.

Request body:

```json
100.00
```

Example response:

```json
{
  "name": "Lucas Silva",
  "document": "52998224725",
  "userType": 1,
  "balance": 100.00
}
```

### `POST /Transfers`

Transfers money from a payer to a payee.

Request body:

```json
{
  "payerId": "019fd90d-97c1-720c-812b-f502f65f600d",
  "payeeId": "019fd90d-e427-74cd-aaf7-a6464f779375",
  "value": 25.50
}
```

Possible success responses:

```text
Transfer completed and notification sent successfully.
```

```text
Transfer completed but notification could not be sent.
```

Possible failure response:

```text
Transfer cannot be completed because authorization not granted.
```

## Postman Collection

The repository includes a Postman collection:

```text
PaymentSimplifier.postman_collection.json
```

It contains ready-to-use requests for the current endpoints.
