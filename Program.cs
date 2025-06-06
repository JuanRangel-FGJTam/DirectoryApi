using System;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Minio;
using Serilog;
using AuthApi.Data;
using AuthApi.Helper;
using AuthApi.Services;

// Define the default culture and supported cultures
var defaultCulture = new CultureInfo("es-MX");
var supportedCultures = new[] {"es-MX","en-US"};

var builder = WebApplication.CreateBuilder(args);

// * configure logger
var serilogConfiguration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(serilogConfiguration).CreateLogger();
builder.Host.UseSerilog();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(defaultCulture);
    options.SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
    options.SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()  // Allow all origins
                   .AllowAnyHeader()  // Allow any header
                   .AllowAnyMethod(); //
        });
});

// Add services to the container.
builder.Services.Configure<JwtSettings>( builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<ResetPasswordSettings>( builder.Configuration.GetSection("ResetPasswordSettings"));
builder.Services.Configure<EmailSettings>( builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<WelcomeEmailSources>( builder.Configuration.GetSection("WelcomeEmailSources"));
builder.Services.Configure<MinioSettings>( builder.Configuration.GetSection("MinioSettings"));
builder.Services.AddJwtAuthentication( builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()! );
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers()
    .AddJsonOptions( o => {
        o.JsonSerializerOptions.AllowTrailingCommas = true;
        o.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });


builder.Services.AddDbContext<DirectoryDBContext>( options=>{
    options.UseSqlServer( builder.Configuration.GetConnectionString("AuthApi") );
});
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICryptographyService>( o => 
    new AesCryptographyService( builder.Configuration.GetValue<string>("Secret")!)
);
builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<PreregisterService>();
builder.Services.AddScoped<IEmailProvider, FGJEmailProvider>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<RecoveryAccountService>();
builder.Services.AddScoped<CatalogService>();
builder.Services.AddScoped<PersonBanService>();
builder.Services.AddScoped<EmailNotificationsService>();
builder.Services.AddSingleton<ResetPasswordState>();
builder.Services.AddSingleton<ChangeEmailState>();
builder.Services.AddMySwaggerConfig();
builder.Services.ConfigureHttpJsonOptions( o => {
    o.SerializerOptions.AllowTrailingCommas = true;
});
builder.Services.AddMinio(client => client
    .WithEndpoint( builder.Configuration["MinioSettings:Endpoint"])
    .WithCredentials(builder.Configuration["MinioSettings:AccessKey"], builder.Configuration["MinioSettings:SecretKey"])
    .WithSSL(false)
    .Build()
);
builder.Services.AddScoped<MinioService>();
builder.Services.ConfigureQuartz();
builder.Services.AddUbicanosServices(builder.Configuration);

var app = builder.Build();
var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);
app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI( c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FGJTam Directory API") );
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<UserLoggingMiddleware>();
app.Run();