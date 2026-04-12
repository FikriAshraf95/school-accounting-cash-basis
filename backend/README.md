# School Accounting API

A RESTful API for school financial management built with .NET 10 and SQL Server.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB, Express, or full edition)

## Getting Started

### 1. Clone and navigate to the project

```bash
cd backend/SchoolAccounting.Api
```

### 2. Configure the database

Ensure SQL Server is running. The default connection string in `appsettings.json` uses Windows Authentication with LocalDB:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=SchoolAccounting;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Update the connection string if needed for your SQL Server setup.

### 3. Run the application

```bash
dotnet run
```

The API will:
- Apply any pending EF Core migrations automatically
- Seed an admin user if no admin exists

### 4. Default admin credentials

On first run, an admin user is created:
- **Email:** admin@school.com
- **Username:** admin
- **Password:** ChangeMe123! (or the value from `AdminSeedPassword` config)

**Important:** Change the admin password immediately after first login.

### 5. API documentation

Once running, the API is available at:
- Base URL: `https://localhost:5001` or `http://localhost:5000`
- API routes: `/api/v1/...`

## API Endpoints

### Authentication

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/v1/register` | Register a new user (role = Viewer) | No |
| POST | `/api/v1/login` | Login and receive bearer token | No |
| POST | `/api/v1/logout` | Revoke current token | Yes |
| GET | `/api/v1/user` | Get current user info | Yes |

### User Management (Admin only)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/users` | List users (paginated) |
| GET | `/api/v1/users/{id}` | Get user details |
| POST | `/api/v1/users` | Create new user |
| PUT | `/api/v1/users/{id}` | Update user |
| PUT | `/api/v1/users/{id}/role` | Assign role |
| PUT | `/api/v1/users/{id}/password` | Change password |
| DELETE | `/api/v1/users/{id}` | Delete user |

## Authentication

This API uses bearer tokens stored in the database (not JWT). Include the token in requests:

```
Authorization: Bearer <token>
```

Tokens expire after 30 days of inactivity.

## Roles

| Role | Access |
|------|--------|
| Admin | Full access including user management |
| Accountant | All financial operations; no user management |
| Staff | Student/payer CRUD + create/edit transactions; no ledger or year-end |
| Viewer | Read-only everywhere; default on registration |

## Project Structure

```
SchoolAccounting.Api/
├── Infrastructure/
│   ├── Persistence/        # EF Core DbContext, migrations
│   ├── Auth/               # Bearer token authentication handler
│   └── Middleware/         # Exception handling middleware
├── Common/                 # Shared utilities, exceptions, roles
└── Features/               # Vertical slice features
    ├── Auth/               # Authentication endpoints
    └── UserManagement/     # Admin user management
```

## Development

### Run migrations

```bash
dotnet ef migrations add <MigrationName>
```

### Run tests

```bash
dotnet test
```

## Configuration

Key settings in `appsettings.json`:

| Setting | Description |
|---------|-------------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `AdminSeedPassword` | Default password for seeded admin account |
| `Logging:LogLevel` | Logging verbosity |

## License

Private - School Accounting System
