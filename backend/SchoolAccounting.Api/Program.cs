using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SchoolAccounting.Api.Infrastructure.Auth;
using SchoolAccounting.Api.Infrastructure.Middleware;
using SchoolAccounting.Api.Infrastructure.Persistence;
using SchoolAccounting.Api.Features.Auth;
using SchoolAccounting.Api.Features.UserManagement;
using SchoolAccounting.Api.Features.Grades;
using SchoolAccounting.Api.Features.Classes;
using SchoolAccounting.Api.Features.Ledgers;
using SchoolAccounting.Api.Features.Categories;
using SchoolAccounting.Api.Features.BusinessInfo;
using SchoolAccounting.Api.Features.Students;
using SchoolAccounting.Api.Features.Payers;
using SchoolAccounting.Api.Features.Transactions;
using SchoolAccounting.Api.Features.JournalEntries;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Add EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add custom authentication
builder.Services.AddAuthentication("Bearer")
    .AddScheme<AuthenticationSchemeOptions, BearerTokenAuthHandler>("Bearer", null);

builder.Services.AddAuthorization();

// Add application services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserManagementService>();
builder.Services.AddScoped<GradeService>();
builder.Services.AddScoped<ClassService>();
builder.Services.AddScoped<LedgerService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<BusinessInfoService>();
builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<PayerService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<TransactionReversalService>();
builder.Services.AddScoped<JournalEntryService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.MapScalarApiReference();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    await DbInitializer.SeedAsync(dbContext, configuration);
}

app.Run();
