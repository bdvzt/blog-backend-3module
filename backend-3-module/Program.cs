using System.Text;
using backend_3_module.Data;
using backend_3_module.Data.DataBases;
using Microsoft.EntityFrameworkCore;
using backend_3_module.Helpers;
using backend_3_module.Jobs;
using backend_3_module.Middlewares;
using backend_3_module.Models.Address;
using backend_3_module.Services;
using backend_3_module.Services.IServices;
using Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Quartz;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Blog")));
builder.Services.AddDbContext<AddressDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Gar")));
builder.Services.AddSingleton(
    new RedisDbContext(builder.Configuration.GetConnectionString("Redis")));

var emailConfig = builder.Configuration
    .GetSection("EmailConfiguration")
    .Get<EmailConfiguration>();
builder.Services.AddSingleton(emailConfig);

builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();
    q.AddJob<EmailJob>(opts =>
        opts.WithIdentity("EmailJob")
            .StoreDurably()
    );
    q.AddTrigger(opts => opts
        .ForJob("EmailJob")
        .WithIdentity("EmailsTrigger")
        .StartNow()
        .WithSimpleSchedule(x => x
            .WithIntervalInSeconds(20)
            .RepeatForever())
    );
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
builder.Services.AddControllers();

builder.Services.AddScoped<DataSeeder>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICommunityService, CommunityService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IAddressesService, AddressesService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddSingleton<Token>();
builder.Services.AddScoped<AddressExsists>();
builder.Services.AddHttpContextAccessor();

var key = builder.Configuration.GetValue<string>("ApiSettings:Secret");

builder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
            "Enter TOKEN here",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new
                    OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using var scopeService = app.Services.CreateScope();
var dbContext = scopeService.ServiceProvider.GetRequiredService<BlogDbContext>();
dbContext.Database.Migrate();

using var scope = app.Services.CreateScope();
var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
await seeder.Seed();

app.UseMiddleware<ExeptionHandlingMiddleWare>();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.Run();