# ASP.NET MVC Project Guidelines

## Project Overview

This project is **JobSeeker**, an ASP.NET Core MVC web application targeting **.NET 10.0**, built and maintained in **Visual Studio**. All code changes must remain compatible with Visual Studio and the standard ASP.NET Core MVC project structure.

The system has **four user roles**:

- **Admin** — manages the platform, users, and overall system settings
- **CareerCounseller** — provides career guidance, reviews applications, and counsels job seekers
- **Employer** — posts job listings and manages applicants
- **Employee** — (job seeker) searches and applies for jobs

---

## Prescribed File Structure

All new files must follow this role-based folder layout. Do not place role-specific code in the root-level folders.

```
JobSeeker/
├── Controllers/
│   ├── Admin/
│   │   └── AdminDashboardController.cs
│   ├── CareerCounseller/
│   │   └── CounsellerDashboardController.cs
│   ├── Employer/
│   │   └── EmployerDashboardController.cs
│   ├── Employee/
│   │   └── EmployeeDashboardController.cs
│   └── HomeController.cs              ← shared/public pages only
│
├── Models/
│   ├── Admin/
│   ├── CareerCounseller/
│   ├── Employer/
│   ├── Employee/
│   └── Shared/                        ← ErrorViewModel and other cross-role models
│
├── Views/
│   ├── Admin/
│   ├── CareerCounseller/
│   ├── Employer/
│   ├── Employee/
│   ├── Home/                          ← public-facing pages (Index, Privacy)
│   └── Shared/                        ← _Layout, _ValidationScriptsPartial, Error
│
├── wwwroot/
│   ├── css/
│   │   ├── site.css                   ← global styles
│   │   ├── admin.css
│   │   ├── counseller.css
│   │   ├── employer.css
│   │   └── employee.css
│   ├── js/
│   │   ├── site.js                    ← global scripts
│   │   ├── admin.js
│   │   ├── counseller.js
│   │   ├── employer.js
│   │   └── employee.js
│   └── lib/                           ← third-party libs (Bootstrap, jQuery), do not modify
│
├── Program.cs
├── appsettings.json
└── JobSeeker.csproj
```

---

## Role Implementation Rules

### General

- Every controller in a role subfolder must use a matching **area-style namespace**, e.g. `namespace JobSeeker.Controllers.Admin`.
- Role-specific routes should follow the pattern `/{Role}/{Controller}/{Action}`, e.g. `/Admin/Dashboard/Index`.
- Use **ASP.NET Core Authorization** with role-based policies. Decorate controllers with `[Authorize(Roles = "Admin")]` (or the relevant role name).
- Role names used in `[Authorize]` attributes must exactly match: `"Admin"`, `"CareerCounseller"`, `"Employer"`, `"Employee"`.

### Admin

- Can access all areas of the system.
- Responsible for user management, role assignments, and system configuration.
- Admin controllers go in `Controllers/Admin/`, models in `Models/Admin/`, views in `Views/Admin/`.

### CareerCounseller

- Can view Employee profiles and job applications.
- Can provide counselling notes and feedback.
- CareerCounseller controllers go in `Controllers/CareerCounseller/`, models in `Models/CareerCounseller/`, views in `Views/CareerCounseller/`.

### Employer

- Can create, edit, and delete their own job listings.
- Can view and manage applications submitted to their listings.
- Employer controllers go in `Controllers/Employer/`, models in `Models/Employer/`, views in `Views/Employer/`.

### Employee

- Can register, build a profile, search job listings, and submit applications.
- Can view counselling feedback directed at them.
- Employee controllers go in `Controllers/Employee/`, models in `Models/Employee/`, views in `Views/Employee/`.

---

## Visual Studio Compatibility Rules

- Always preserve the `.slnx` solution file (`JobSeeker.slnx`) and `.csproj` project file (`JobSeeker/JobSeeker.csproj`). Do not replace or remove them.
- Do not introduce build tooling (e.g. Vite, webpack config, custom MSBuild scripts) that conflicts with Visual Studio's default build pipeline.
- Keep the `TargetFramework` as `net10.0` unless explicitly instructed to upgrade.
- Do not modify `Properties/launchSettings.json` in a way that breaks F5 debugging in Visual Studio.
- Preserve the `appsettings.json` and `appsettings.Development.json` pattern for configuration.

## ASP.NET Core MVC Conventions

- Follow the **MVC folder convention** with the role-based subfolders described above.
- Controller classes must inherit from `Controller` or `ControllerBase` (for API controllers).
- View files use the `.cshtml` Razor extension. Do not use `.html` or other templating formats.
- Strongly-typed views should use `@model` directives and corresponding ViewModel/Model classes.
- Use **Tag Helpers** and **HTML Helpers** for form elements and links in Razor views.
- Register all services via `builder.Services` in `Program.cs` using the built-in DI container.
- Middleware must be added in `Program.cs` in the correct pipeline order (routing → auth → endpoints).

## C# Coding Standards

- Use C# 13 language features where appropriate (`<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`).
- Enable and respect nullable reference types — avoid `null!` suppression unless justified.
- Use `async`/`await` for any I/O-bound operations (database, file, HTTP calls).
- Follow PascalCase for public members and camelCase for local variables and parameters.

## Static Assets

- Static files (CSS, JS, images) belong under `wwwroot/`.
- Role-specific CSS and JS go in role-named files (e.g. `wwwroot/css/employer.css`).
- Reference static assets using `~/<path>` or Tag Helpers (`asp-append-version="true"`).
- The project uses `MapStaticAssets()` — do not replace it with `UseStaticFiles()` without a specific reason.

## Database & Data Access

- Prefer **Entity Framework Core** with Code-First migrations (compatible with Visual Studio's Package Manager Console: `Add-Migration`, `Update-Database`).
- Connection strings belong in `appsettings.json` under `"ConnectionStrings"`.
- Each role should have its own DbSet groupings or at minimum clearly named entities (e.g. `JobListing`, `Application`, `CounsellingNote`).

## What to Avoid

- Do not add `node_modules` or npm-based build steps as the primary asset pipeline.
- Do not switch the project to Minimal API only — keep `AddControllersWithViews()` and the MVC route pattern.
- Do not remove or alter the default MVC route in `Program.cs` without explicit instruction.
- Do not place role-specific controllers, models, or views directly in the root `Controllers/`, `Models/`, or `Views/` folders — use the role subfolders.
- Avoid referencing packages not available on NuGet or that require manual DLL references incompatible with the SDK-style `.csproj`.
