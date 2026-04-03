# Project 1 - EmployeeDepartment API

This project is an ASP.NET Core Web API that satisfies the uploaded Web Engineering assignment requirements using an **Employee / Department / Project** system with authentication and a Hangfire background job.
by: Seif Eldeen Hesham 211004041
## Assignment Requirements Coverage

This project includes all required technical items from the PDF:

- ASP.NET Core Web API
- Entity Framework Core with MySQL
- One-to-one relationship
- One-to-many relationship
- Many-to-many relationship
- At least three services using dependency injection
- Create / Update / Read DTOs
- DTO validation using Data Annotations
- JWT authentication with login endpoint
- Authorization using `[Authorize]`
- Role-based authorization
- LINQ `Select()` projection into DTOs
- `AsNoTracking()` for read-only queries
- Async EF Core methods
- EF Core migration included
- Swagger documentation
- Bonus: Hangfire recurring background job

## Domain Design

### Entities
- `Department`
- `Employee`
- `EmployeeProfile`
- `Project`
- `AppUser`

### Relationships
- **One-to-many:** `Department -> Employees`
- **One-to-one:** `Employee -> EmployeeProfile`
- **Many-to-many:** `Employee <-> Project`

The many-to-many relationship is implemented using EF Core skip navigations, which creates the join table `EmployeeProjects` automatically in the database.

## Project Structure

- `Controllers/` API controllers
- `DTOs/` request and response DTOs
- `Data/` EF Core DbContext
- `Models/` entity models
- `Services/` business logic layer
- `Interfaces/` service contracts
- `Helpers/` password hashing helper
- `Jobs/` Hangfire recurring job
- `Migrations/` EF Core migration files

## Services

The project contains four services, each registered with dependency injection:

- `DepartmentService`
- `EmployeeService`
- `ProjectService`
- `AuthService`

## Authentication and Authorization

### Authentication
Authentication is implemented using JWT Bearer tokens.

Available auth endpoints:
- `POST /api/Auth/register`
- `POST /api/Auth/login`

The login endpoint returns a JWT token that must be sent in the `Authorization` header:

```text
Authorization: Bearer {token}
```

### Authorization
Protected endpoints use `[Authorize]`.

Role-based authorization is also implemented:
- `Admin` can create, update, and delete resources
- `Admin` and `User` can access read endpoints where configured

## DTO Validation

Validation is implemented with Data Annotations such as:
- `Required`
- `MaxLength`
- `MinLength`
- `EmailAddress`
- `Range`
- `Phone`
- `RegularExpression`

Because controllers use `[ApiController]`, invalid requests automatically return **HTTP 400 Bad Request** before database operations are executed.

## LINQ Optimization and Read Performance

Read endpoints use:
- `Select()` to project entities into response DTOs
- `AsNoTracking()` for read-only queries

This prevents returning full EF Core entity objects directly and improves performance.

## Hangfire Background Job

A Hangfire recurring job is included as the bonus part.

### Job Name
`ProjectReminderJob`

### What it does
The job runs daily and logs a simple reminder summary for projects, including:
- project name
- assigned employee count

### Dashboard
Open Hangfire dashboard at:

```text
/hangfire
```

## Default Admin User

A seeded admin user is included for easy testing.

- **Username:** `admin`
- **Password:** `Admin@123`
- **Role:** `Admin`

## Technologies Used

- **ASP.NET Core Web API**: framework for building RESTful APIs.
- **Entity Framework Core**: ORM for querying and updating the database.
- **Pomelo.EntityFrameworkCore.MySql**: MySQL provider for EF Core.
- **MySQL**: relational database used by the application.
- **JWT (JSON Web Token)**: secure token-based authentication mechanism.
- **Swagger / Swashbuckle**: API documentation and interactive endpoint testing.
- **Hangfire**: recurring background job processing.
- **LINQ**: used for projection and query composition.
- **Data Annotations**: input validation on DTOs.

## Why HTTP-Only Cookies Are Commonly Used as Industry Standard

HTTP-only cookies are a critical security feature in web applications that prevent client-side scripts from accessing sensitive cookie data. They are the industry standard for authentication security.

### Key Security Benefits

1. **XSS (Cross-Site Scripting) Protection**
   - HTTP-only cookies cannot be read by JavaScript (`document.cookie` returns empty string)
   - Even if an attacker injects malicious scripts through XSS vulnerabilities, they cannot steal the authentication token
   - This prevents session hijacking and unauthorized account access

2. **Token Storage Security vs. localStorage/sessionStorage**
   - `localStorage` and `sessionStorage` are accessible via JavaScript and vulnerable to XSS attacks
   - Any script running on the page can steal tokens from storage: `localStorage.getItem('token')`
   - HTTP-only cookies are completely inaccessible to JavaScript, eliminating this attack vector

3. **Automatic Browser Handling**
   - Browsers automatically include HTTP-only cookies in requests to the same domain
   - No manual token management required in frontend code
   - Reduces risk of accidental token exposure in client-side code

4. **Secure Flag Combination**
   - When combined with the `Secure` flag, cookies are only sent over HTTPS connections
   - When combined with `SameSite=Strict`, CSRF (Cross-Site Request Forgery) attacks are mitigated
   - Example: `Set-Cookie: auth=xyz; HttpOnly; Secure; SameSite=Strict`

5. **Industry Adoption & Standards**
   - Used by Google, Facebook, banking systems, and all major web applications
   - Recommended by OWASP (Open Web Application Security Project) as best practice
   - Required for compliance with security standards like PCI DSS and SOC 2
   - Part of the HTTP State Management Mechanism standard (RFC 6265)

### How HTTP-Only Cookies Work

**Server Response:**
```
HTTP/1.1 200 OK
Set-Cookie: auth_token=eyJhbGci...; HttpOnly; Secure; SameSite=Strict; Max-Age=3600
```

**Browser Behavior:**
- ✓ Automatically includes cookie in all requests to the same domain
- ✗ JavaScript cannot read: `document.cookie` returns `""`
- ✓ Only transmitted over encrypted HTTPS connections
- ✓ Not sent with cross-origin requests (with SameSite)

### Comparison: HTTP-Only Cookies vs. localStorage

| Feature | HTTP-Only Cookie | localStorage |
|---------|-----------------|--------------|
| JavaScript Access | Blocked |  Full access |
| XSS Vulnerability |  Protected | Vulnerable |
| Automatic Request Inclusion | Yes |  Manual |
| HTTPS Enforcement | With Secure flag | No |
| CSRF Protection | With SameSite | No |
| Token Expiration | Built-in | Manual |

### Why This API Uses JWT in Headers

While this API returns JWT in the response body for the assignment requirements, production browser-based applications should:
1. Store JWT in HTTP-only cookie after login
2. Browser automatically sends cookie with each request
3. Eliminates XSS attack vectors on authentication tokens

This approach provides defense-in-depth against the most common web application vulnerabilities.

## Detailed API Endpoint Documentation

### Authentication Endpoints

#### POST /api/Auth/register
Register a new user account.

**Request Body:**
```json
{
  "fullName": "John Doe",
  "userName": "johndoe",
  "email": "john@example.com",
  "password": "securepassword123",
  "role": "User"
}
```

**Response (201 Created):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-04-03T22:30:00Z",
  "userName": "johndoe",
  "role": "User"
}
```

**Error Responses:**
- `400 Bad Request` - Validation errors (missing fields, invalid email, weak password)
- `409 Conflict` - Username or email already exists

#### POST /api/Auth/login
Authenticate user and receive JWT token.

**Request Body:**
```json
{
  "userName": "admin",
  "password": "admin123"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-04-03T22:30:00Z",
  "userName": "admin",
  "role": "Admin"
}
```

**Error Responses:**
- `401 Unauthorized` - Invalid username or password

### Department Endpoints (Requires Authentication)

#### GET /api/Departments
Retrieve all departments with employee count.

**Headers:**
```
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "name": "Engineering",
    "location": "Building A",
    "employeesCount": 5
  },
  {
    "id": 2,
    "name": "Marketing",
    "location": "Building B",
    "employeesCount": 3
  }
]
```

#### GET /api/Departments/{id}
Retrieve a specific department by ID.

**Response (200 OK):**
```json
{
  "id": 1,
  "name": "Engineering",
  "location": "Building A",
  "employeesCount": 5
}
```

**Error Responses:**
- `404 Not Found` - Department does not exist

#### POST /api/Departments
Create a new department.

**Request Body:**
```json
{
  "name": "Research & Development",
  "location": "Building C"
}
```

**Validation Rules:**
- `name`: Required, 2-100 characters
- `location`: Required, 2-150 characters

**Response (201 Created):**
```json
{
  "id": 3,
  "name": "Research & Development",
  "location": "Building C",
  "employeesCount": 0
}
```

#### PUT /api/Departments/{id}
Update an existing department.

**Request Body:**
```json
{
  "name": "R&D Department",
  "location": "Building D"
}
```

**Response (200 OK):** Updated department object

#### DELETE /api/Departments/{id}
Delete a department.

**Response (204 No Content)**

**Error Responses:**
- `400 Bad Request` - Cannot delete department with assigned employees

### Employee Endpoints (Requires Authentication)

#### GET /api/Employees
Retrieve all employees with department and project information.

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "name": "John Smith",
    "email": "john.smith@company.com",
    "phone": "+1-555-0123",
    "salary": 75000.00,
    "departmentId": 1,
    "departmentName": "Engineering",
    "projects": ["Website Redesign", "Mobile App"]
  }
]
```

#### GET /api/Employees/{id}
Retrieve a specific employee.

#### POST /api/Employees
Create a new employee.

**Request Body:**
```json
{
  "name": "Jane Doe",
  "email": "jane.doe@company.com",
  "phone": "+1-555-0456",
  "salary": 85000,
  "departmentId": 1,
  "profile": {
    "address": "123 Main St, City",
    "dateOfBirth": "1990-05-15",
    "emergencyContact": "+1-555-9999"
  },
  "projectIds": [1, 2]
}
```

**Response (201 Created):** Created employee object

#### PUT /api/Employees/{id}
Update an employee.

#### DELETE /api/Employees/{id}
Delete an employee.

### Project Endpoints (Requires Authentication)

#### GET /api/Projects
Retrieve all projects with assigned employees.

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "name": "Website Redesign",
    "description": "Complete overhaul of company website",
    "startDate": "2026-01-15",
    "endDate": "2026-06-30",
    "budget": 50000.00,
    "employees": [
      { "id": 1, "name": "John Smith" },
      { "id": 2, "name": "Jane Doe" }
    ]
  }
]
```

#### POST /api/Projects
Create a new project.

**Request Body:**
```json
{
  "name": "Mobile Application",
  "description": "Develop iOS and Android applications",
  "startDate": "2026-04-01",
  "endDate": "2026-09-30",
  "budget": 100000
}
```

#### POST /api/Projects/{id}/employees/{employeeId}
Assign an employee to a project.

**Response (200 OK)**

#### DELETE /api/Projects/{id}/employees/{employeeId}
Remove an employee from a project.

**Response (204 No Content)**

### Hangfire Dashboard

#### GET /hangfire
Background job monitoring dashboard.

**Features:**
- View scheduled jobs
- Monitor job execution history
- Retry failed jobs
- View recurring jobs (ProjectReminderJob runs daily)

## API Screenshots Documentation

The following screenshots demonstrate working API endpoints using Swagger UI:

### Screenshot 1: Login Endpoint Success
**Endpoint:** `POST /api/Auth/login`

Shows successful authentication with:
- **Request:** JSON body with username "admin" and password "admin123"
- **Response:** 200 OK status
- **Response Body:** Contains JWT token, expiration time, username, and role

![Login](screenshots/Screenshot%202026-04-03%20020653.png)

- **Curl:**
```bash
curl -X POST "https://localhost:7166/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{"userName":"admin","password":"admin123"}'
```

### Screenshot 2: Swagger Authorization Setup
**Location:** Swagger UI Authorize Dialog

Shows authorization configuration:
- **Bearer Token Input:** JWT token pasted in the value field
- **Available Authorizations:** Bearer (apiKey) shows green checkmark
- **Authorize Button:** Successfully configured
- **Note:** Token is entered without the "Bearer " prefix in Swagger

![Authorize](screenshots/Screenshot%202026-04-03%20020832.png)

### Screenshot 3: Access Protected Endpoint (GET Departments)
**Endpoint:** `GET /api/Departments`

Shows successful access to protected resource:
- **Request Headers:** Authorization: Bearer {token} automatically included
- **Response:** 200 OK
- **Response Body:** Empty array `[]` (no departments created yet)
- **Proof:** Authentication working - no 401 error

![GET Departments](screenshots/Screenshot%202026-04-03%20022641.png)

### Screenshot 4: Create Department (POST)
**Endpoint:** `POST /api/Departments`

Shows resource creation:
- **Request Body:**
```json
{
  "name": "Engineering",
  "location": "Building A"
}
```
- **Response:** 201 Created
- **Response Body:** Created department with ID, name, location, employeesCount: 0

![Create Department](screenshots/Screenshot%202026-04-03%20022732.png)

### Screenshot 5: Get Department by ID
**Endpoint:** `GET /api/Departments/1`

Shows retrieval of specific resource:
- **Response:** 200 OK
- **Response Body:** Department with ID, name, location, and employee count
- **Proof:** Resource created in Screenshot 4 is now retrievable

![GET Department by ID](screenshots/Screenshot%202026-04-03%20002752.png)
### Screenshot 6: 401 Unauthorized Error (No Token)
**Endpoint:** `GET /api/Departments` (without authorization)

Shows authentication enforcement:
- **Request:** No Authorization header sent
- **Response:** 401 Unauthorized
- **Response Headers:** WWW-Authenticate: Bearer error="invalid_token"
- **Response Body:** Empty (or error details)
- **Proof:** Protected endpoints correctly reject unauthenticated requests

![401 Error](screenshots/Screenshot%202026-04-03%20023426.png)

### Screenshot 7: Validation Error (400 Bad Request)
**Endpoint:** `POST /api/Departments`

Shows DTO validation working:
- **Request Body:** Missing required fields or invalid data
```json
{
  "name": "A",
  "location": ""
}
```
- **Response:** 400 Bad Request
- **Response Body:** Validation error details:
```json
{
  "errors": {
    "Name": ["The field Name must be a string with a minimum length of 2 and a maximum length of 100."],
    "Location": ["The Location field is required."]
  }
}
```

![Validation Error](screenshots/Screenshot%202026-04-03%20022811-1.png)

### Screenshot 8: Hangfire Dashboard
**URL:** `https://localhost:7166/hangfire`

Shows background job monitoring:
- **Recurring Jobs:** ProjectReminderJob listed with Cron expression (Daily)
- **Job History:** Previous job executions
- **Servers:** Connected Hangfire server instances
- **Queues:** Job queue status

![Hangfire](screenshots/Screenshot%202026-04-03%20002811.png)

## Endpoints Screenshots

The following screenshots demonstrate actual API endpoint testing in Swagger UI:

### 1. POST /api/Projects - Create Project
![POST Projects](screenshots/Screenshot%202026-04-03%20002547.png)

Shows creating a new project with:
- **Request Body:** Project details (name, description, budget, dates)
- **Response:** 201 Created with created project including ID and employee count
- **Authorization:** Bearer token included in request

### 2. GET /api/Projects - List All Projects
![GET Projects](screenshots/Screenshot%202026-04-03%20002619.png)

Shows retrieving all projects:
- **Response:** 200 OK with array of projects
- **Response Body:** Project details including assigned employees

### 3. PUT /api/Employees/{id} - Update Employee
![PUT Employee](screenshots/Screenshot%202026-04-03%20002640.png)

Shows updating an existing employee:
- **Parameter:** Employee ID = 1
- **Request Body:** Updated employee details with profile and project assignments
- **Response:** 204 No Content (successful update)

### 4. POST /api/Employees - Create Employee
![POST Employee](screenshots/Screenshot%202026-04-03%20002711.png)

Shows creating a new employee:
- **Request Body:** Employee data with nested profile object and project IDs
- **Response:** 201 Created with full employee object including ID

### 5. GET /api/Employees - List All Employees
![GET Employees](screenshots/Screenshot%202026-04-03%20002730.png)

Shows retrieving all employees:
- **Response:** 200 OK with array of employees
- **Response Body:** Full employee details including department name, profile, and assigned projects

### 6. POST /api/Auth/login - Authentication
![Login](screenshots/Screenshot%202026-04-03%20020653.png)

Shows successful login with admin credentials:
- **Request:** username "admin", password "admin123"
- **Response:** 200 OK with JWT token, expiration, username, and role

### 7. Swagger Authorization Dialog
![Authorize](screenshots/Screenshot%202026-04-03%20020832.png)

Shows how to authorize in Swagger:
- **Bearer Token:** JWT token entered in the value field
- **Available Authorizations:** Shows green checkmark when authorized

### 8. Hangfire Dashboard
![Hangfire](screenshots/Screenshot%202026-04-03%20002811.png)

Shows background job monitoring dashboard with recurring jobs listed

## How to Run the Project

### 1. Configure the database connection
Edit `appsettings.json` and update the MySQL connection string if needed:

```json
"ConnectionStrings": {
  "DefaultConnection": "server=localhost;port=3306;database=employee_department_db;user=root;password=your_password"
}
```

### 2. Restore packages
Run this inside the project folder:

```bash
dotnet restore Project1.csproj
```

### 3. Apply migrations
```bash
dotnet ef database update --project Project1.csproj --startup-project Project1.csproj
```

### 4. Run the API
```bash
dotnet run --project Project1.csproj
```

### 5. Open Swagger
After running the API, open:

```text
https://localhost:{port}/swagger
```

### 6. Open Hangfire dashboard
```text
https://localhost:{port}/hangfire
```

## Testing Flow

Recommended test order:

1. Open Swagger
2. Call `POST /api/Auth/login` using the seeded admin account
3. Copy the returned JWT token
4. Click **Authorize** in Swagger and paste `Bearer {token}`
5. Create a department
6. Create a project
7. Create an employee with profile and project assignments
8. Test read, update, and delete endpoints
9. Open `/hangfire` and verify the recurring job exists

## Main Endpoints

### Auth
- `POST /api/Auth/register`
- `POST /api/Auth/login`

### Departments
- `GET /api/Departments`
- `GET /api/Departments/{id}`
- `POST /api/Departments`
- `PUT /api/Departments/{id}`
- `DELETE /api/Departments/{id}`

### Employees
- `GET /api/Employees`
- `GET /api/Employees/{id}`
- `POST /api/Employees`
- `PUT /api/Employees/{id}`
- `DELETE /api/Employees/{id}`

### Projects
- `GET /api/Projects`
- `GET /api/Projects/{id}`
- `POST /api/Projects`
- `PUT /api/Projects/{id}`
- `DELETE /api/Projects/{id}`
