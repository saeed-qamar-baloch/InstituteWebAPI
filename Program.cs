using InstituteWebAPI.Data;
using InstituteWebAPI.Mappings;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebAPI.Repositories.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;
using InstituteWebAPI.Services.TermContext;
using InstituteWebAPI.Services.StudentMonthlyResults;
using InstituteWebAPI.Services.FeeManagement;
using InstituteWebAPI.Models.Configuration;
using InstituteWebAPI.BackgroundJobs;
using InstituteWebAPI.Services.Storage;
using InstituteWebAPI.Services.Sms;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Load local dev overrides (gitignored — never committed).
// Provides Jwt:Key and connection string overrides for local development.
// In production use environment variables or host config instead.
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);

// ── CORS ─────────────────────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173", "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("RozhnCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ── Rate Limiting ─────────────────────────────────────────────────────────────
// "login" policy: max 10 attempts per IP per 5 minutes.
// Prevents password-spray attacks on the login endpoint.
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", o =>
    {
        o.PermitLimit = 10;
        o.Window = TimeSpan.FromMinutes(5);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Add services to the container.

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .SelectMany(entry => entry.Value!.Errors.Select(error =>
                    string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? $"The {entry.Key} field is invalid."
                        : error.ErrorMessage))
                .ToArray();

            return new BadRequestObjectResult(new
            {
                Message = "Validation failed.",
                Errors = errors
            });
        };
    });
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ImageStorage>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Rozhn Institute API",
        Version = "v1"
    });
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id= JwtBearerDefaults.AuthenticationScheme
            },
            Scheme = "Oauth2",
            Name = JwtBearerDefaults.AuthenticationScheme,
            In = ParameterLocation.Header
        },
        new List<string>()
        }
    });
});

builder.Services.AddDbContext<RozhnInstituteDbContext>(options =>
    options
        .UseSqlServer(builder.Configuration.GetConnectionString("RozhnWebConnectionString"))
        .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

builder.Services.AddDbContext<RozhnInstituteAuthDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("RozhnWebAuthConnectionString")));

builder.Services.Configure<FeeManagementOptions>(builder.Configuration.GetSection("FeeManagement"));
builder.Services.Configure<InstituteWebAPI.Models.Configuration.SmsGatewaySettings>(builder.Configuration.GetSection("SmsGateway"));
builder.Services.AddHttpClient<ISmsService, HttpSmsService>();

builder.Services.AddScoped<ITermRepository, TermRepository>();
builder.Services.AddScoped<ITermMonthsRepository, TermMonthsRepository>();
builder.Services.AddScoped<ICoursesRepository, CoursesRepository>();
builder.Services.AddScoped<IWebsitePostRepository, WebsitePostRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<ILearnerRepository, LearnerRepository>();
builder.Services.AddScoped<IPasswordHasher<InstituteWebApp.Models.Domain.Learner>, PasswordHasher<InstituteWebApp.Models.Domain.Learner>>();
builder.Services.AddScoped<IVillageRepository, VillageRepository>();
builder.Services.AddScoped<IClassesRepository, ClassesRepository>();
builder.Services.AddScoped<ISlotsRepository, SlotsRepository>();
builder.Services.AddScoped<ISectionRepository, SectionRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<ITeacherCoursesRepository, TeacherCoursesRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IAdmissionsRepository, AdmissionsRepository>();
builder.Services.AddScoped<ICurrentClassRepository, CurrentClassRepository>();
builder.Services.AddScoped<ITestsRepository, TestsRepository>();
builder.Services.AddScoped<IStudentMarksRepository, StudentMarksRepository>();
builder.Services.AddScoped<IClassStudentsRepository, ClassStudentsRepository>();
builder.Services.AddScoped<IFeeTypeRepository, FeeTypeRepository>();
builder.Services.AddScoped<ITestTypeRepository, TestTypeRepository>();
builder.Services.AddScoped<ITeacherIdentityLinkRepository, TeacherIdentityLinkRepository>();
builder.Services.AddScoped<IFeeManagementRepository, FeeManagementRepository>();
builder.Services.AddScoped<IGuardianRepository, GuardianRepository>();
builder.Services.AddScoped<IScholarshipRepository, ScholarshipRepository>();
builder.Services.AddScoped<IResultApprovalRepository, ResultApprovalRepository>();
builder.Services.AddScoped<IMarkEditRequestRepository, MarkEditRequestRepository>();
builder.Services.AddScoped<ITeacherDailyAttendanceRepository, TeacherDailyAttendanceRepository>();
builder.Services.AddScoped<ITermContext, TermContext>();
builder.Services.AddScoped<InstituteWebAPI.Services.Audit.IAuditService, InstituteWebAPI.Services.Audit.AuditService>();
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, InstituteWebAPI.Services.Access.AreaPermissionHandler>();
builder.Services.AddScoped<IStudentMonthlyResultService, StudentMonthlyResultService>();
builder.Services.AddScoped<IFeeManagementService, FeeManagementService>();
builder.Services.AddScoped<InstituteWebAPI.Services.Notifications.IAppNotificationService, InstituteWebAPI.Services.Notifications.AppNotificationService>();
builder.Services.AddHostedService<MonthlyFeeGenerationJob>();


builder.Services.AddScoped<ITokenRepository, TokenRepository>();


builder.Services.AddAutoMapper(typeof(AutoMapperProfiles));



builder.Services.AddIdentityCore<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddTokenProvider<DataProtectorTokenProvider<IdentityUser>>("RozhnInstitute")
    .AddEntityFrameworkStores<RozhnInstituteAuthDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Minimum viable policy: 8 chars + at least one digit.
    // Upper/lower/special kept off to avoid frustrating non-Latin keyboard users.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;
});


// Fail fast if the JWT signing key is missing or too weak for HS256.
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key is missing or too short. Set a strong secret (32+ chars) via configuration " +
        "or the Jwt__Key environment variable before starting the API.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = builder.Configuration["Jwt:Issuer"],
    ValidAudience = builder.Configuration["Jwt:Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
});

// SECURITY: every endpoint requires an authenticated user UNLESS it is explicitly
// marked [AllowAnonymous]. Closes any controller/action missing an [Authorize]
// attribute (defence-in-depth on top of the per-controller role gates).
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// Trust X-Forwarded-Proto from nginx so req.Scheme = "https" in all controllers
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Auto-apply any pending EF Core migrations on startup.
// All schema changes are now managed exclusively via EF migrations —
// see Migrations/ for the full history.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RozhnInstituteDbContext>();
    await db.Database.MigrateAsync();
}

// Apply Identity / auth schema migrations (AspNetUsers, AspNetRoles, etc.)
using (var scope = app.Services.CreateScope())
{
    var authDb = scope.ServiceProvider.GetRequiredService<RozhnInstituteAuthDbContext>();
    await authDb.Database.MigrateAsync();
}

// Seed roles + default admin user
await AuthDbSeeder.SeedAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Production: return a clean JSON error (no stack traces) and log the exception.
    app.UseExceptionHandler(errApp =>
    {
        errApp.Run(async context =>
        {
            var feature = context.Features.Get<IExceptionHandlerPathFeature>();
            var logger = context.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("GlobalExceptionHandler");
            logger.LogError(feature?.Error, "Unhandled exception on {Method} {Path}",
                context.Request.Method, context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                message = "An unexpected error occurred. Please try again."
            });
        });
    });
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors("RozhnCorsPolicy");

app.UseRateLimiter();

// Public content/profile images are served BEFORE auth so the global fallback
// authorization policy does not block them (they must stay publicly readable).
// IMPORTANT: use ContentRootPath (same base every upload controller/repository
// uses to SAVE these files), not Directory.GetCurrentDirectory() — under systemd
// the process CWD depends on WorkingDirectory= and can differ from ContentRootPath,
// which would make this look in the wrong folder, find nothing, and fall through
// to the global auth FallbackPolicy (401) instead of serving the file.
// Ensure Images subdirectories exist — prevents crash if deploy skips them
var imageStorage = app.Services.GetRequiredService<ImageStorage>();
var imagesRoot = imageStorage.RootPath;
Directory.CreateDirectory(Path.Combine(imagesRoot, "Students"));
Directory.CreateDirectory(Path.Combine(imagesRoot, "Teachers"));
Directory.CreateDirectory(Path.Combine(imagesRoot, "Institute"));
Directory.CreateDirectory(Path.Combine(imagesRoot, "Website"));

// PUBLIC images only: website content (Website) and the institute logo (Institute).
// Student/Teacher photos are PII and are NOT served here — they are behind
// /api/secure-image which requires an authenticated Admin/Teacher.
foreach (var publicFolder in new[] { "Website", "Institute" })
{
    var dir = Path.Combine(imagesRoot, publicFolder);
    Directory.CreateDirectory(dir);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(dir),
        RequestPath = "/images/" + publicFolder,
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers["Access-Control-Allow-Origin"] = "*";
            ctx.Context.Response.Headers["Access-Control-Allow-Methods"] = "GET";
        }
    });
}

// A missing public image must be a 404. Without this terminal check the request
// reaches the global authorization fallback and is incorrectly reported as 401.
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/images/Website") ||
        context.Request.Path.StartsWithSegments("/images/Institute"))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
