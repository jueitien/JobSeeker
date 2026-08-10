using Amazon;
using Amazon.S3;
using JobSeeker.Services;
using JobSeeker.Data;
using JobSeeker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "DefaultConnection was not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.ConfigureWarnings(w =>
        w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Amazon S3 client.
// No AWS access key, secret key, or session token is stored in the project.
// The AWS SDK for .NET resolves credentials using its normal credential chain.
// During local development, place the AWS Academy Learner Lab credentials in:
//   Windows: %USERPROFILE%\.aws\credentials
// under the [default] profile.
builder.Services.AddSingleton<IAmazonS3>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    var regionName = configuration["AWS:Region"]?.Trim();
    if (string.IsNullOrWhiteSpace(regionName))
    {
        regionName = Environment.GetEnvironmentVariable("AWS_REGION")
            ?? Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION")
            ?? "us-east-1";
    }

    var region = RegionEndpoint.GetBySystemName(regionName);

    // No credentials are passed here intentionally. AmazonS3Client will use
    // the AWS SDK credential provider chain, including the shared
    // %USERPROFILE%\.aws\credentials file during local development.
    return new AmazonS3Client(region);
});

builder.Services.AddScoped<S3StorageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync();

        await IdentitySeeder.SeedRolesAsync(
            scope.ServiceProvider);

        await IdentitySeeder.SeedAdminUserAsync(
            scope.ServiceProvider);

        // Fake seed data removed — use real registrations for testing.
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization failed: {ex.Message}");
        throw;
    }
}

app.Run();
