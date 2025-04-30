using InstituteWebAPI.Data;
using InstituteWebAPI.Mappings;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebAPI.Repositories.Repository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options=>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title ="Rozhn Institute API", Version="v1"
    });
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name ="Authorization",
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

builder.Services.AddDbContext<RozhnInstituteDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("RozhnWebConnectionString")));

builder.Services.AddDbContext<RozhnInstituteAuthDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("RozhnWebAuthConnectionString")));
builder.Services.AddScoped<ITermRepository, TermRepository>();
builder.Services.AddScoped<ITermMonthsRepository, TermMonthsRepository>();
builder.Services.AddScoped<ICoursesRepository, CoursesRepository>();
builder.Services.AddScoped<IVillageRepository, VillageRepository>();
builder.Services.AddScoped<IClassesRepository, ClassesRepository>();
builder.Services.AddScoped<ISectionsRepository, SectionsRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<ITeacherCoursesRepository, TeacherCoursesRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IAdmissionsRepository, AdmissionsRepository>();
builder.Services.AddScoped<ICurrentClassRepository, CurrentClassRepository>();
builder.Services.AddScoped< ITestsRepository, TestsRepository>();
builder.Services.AddScoped<IStudentMarksRepository, StudentMarksRepository>();
builder.Services.AddScoped<IClassStudentsRepository, ClassStudentsRepository>();


builder.Services.AddScoped<ITokenRepository, TokenRepository>();


builder.Services.AddAutoMapper(typeof(AutoMapperProfiles));



builder.Services.AddIdentityCore<IdentityUser>().AddRoles<IdentityRole>().AddTokenProvider<DataProtectorTokenProvider<IdentityUser>>("RozhnInstitute").AddEntityFrameworkStores<RozhnInstituteAuthDbContext>().AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = builder.Configuration["Jwt:Issuer"],
    ValidAudience = builder.Configuration["Jwt:Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles(
    new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Images")),
        RequestPath= "/images"
    }
    
    );

app.MapControllers();

app.Run();
