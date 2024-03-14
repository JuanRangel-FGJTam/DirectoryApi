using Microsoft.EntityFrameworkCore;
using AuthApi.Data;
using AuthApi.Helper;
using AuthApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<JwtSettings>( builder.Configuration.GetSection("JwtSettings"));
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
builder.Services.AddMySwaggerConfig();
builder.Services.ConfigureHttpJsonOptions( o => {
    o.SerializerOptions.AllowTrailingCommas = true;
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI( c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FGJTam Directory API") );
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();