using InstituteWebAPI.Data;
using InstituteWebAPI.Mappings;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebAPI.Repositories.Repository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<RozhnInstituteDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("RozhnWebConnectionString")));
builder.Services.AddScoped<ITermRepository, TermRepository>();
builder.Services.AddScoped<ITermMonthsRepository, TermMonthsRepository>();

builder.Services.AddScoped<ICoursesRepository, CoursesRepository>();
builder.Services.AddScoped<IVillageRepository, VillageRepository>();
builder.Services.AddAutoMapper(typeof(AutoMapperProfiles));
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
