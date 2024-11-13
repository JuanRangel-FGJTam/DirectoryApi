using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel.Args;
using AuthApi.Data;
using AuthApi.Helper;
using AuthApi.Services;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddJwtAuthentication( builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()! );
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers().AddJsonOptions( o => {
    o.JsonSerializerOptions.AllowTrailingCommas = true;
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

var app = builder.Build();
app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI( c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FGJTam Directory API") );
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();