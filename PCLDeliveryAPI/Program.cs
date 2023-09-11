using Amazon.S3;
using AutoWrapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PCLDeliveryAPI;
using PorscheComponent.Component;
using PorscheComponent.Interface;
using PorscheDataAccess.PorscheContext;
using PorscheDataAccess.Repositories;
using System.Text;


var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,    
    WebRootPath = "wwwroot"
});

// Add builder.Services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
       x => x.AllowAnyOrigin()
             .AllowAnyMethod()
             .AllowAnyHeader());
});

builder.Services.AddSwaggerGen();

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();

string environment = builder.Configuration.GetSection("Environment").Value;
string dbSecretName = builder.Configuration.GetSection("DBSecretName").Value;
string region = builder.Configuration.GetSection("Region").Value;

if (environment == "DEV")
{
    builder.Services.AddDbContext<PorscheDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("PorscheDbConnection")));
}
else
{
    #region Get Database connection values from AWS Secret manager

    DatabaseSecrets databaseSecrets = SecretsManager.GetSecret(dbSecretName, region);

    string dbsecrets = string.Empty;

    if (databaseSecrets != null)
    {
        dbsecrets = "User ID=" + databaseSecrets.username + ";Password=" + databaseSecrets.password + ";Server=" + databaseSecrets.host + ";Port=" + databaseSecrets.port + "; Database=" + databaseSecrets.dbname + ";Integrated Security=false;Pooling=true;";
    }

    #endregion

    builder.Services.AddDbContext<PorscheDbContext>(options => options.UseNpgsql(dbsecrets));
}

builder.Services.AddScoped<ISecretsManagerComponent, SecretsManagerComponent>();
builder.Services.AddScoped<IUserComponent, UserComponent>();
builder.Services.AddScoped<IDeliveryComponent, DeliveryComponent>();
builder.Services.AddScoped<ISurveyComponent, SurveyComponent>();
builder.Services.AddScoped<IAdminComponent, AdminComponent>();
builder.Services.AddScoped<ICentreComponent, CentreComponent>();
builder.Services.AddScoped<ICarModelComponent, CarModelComponent>();
builder.Services.AddScoped<IRoleModuleComponent, RoleModuleComponent>();
builder.Services.AddScoped<IAuditLogComponent, AuditLogComponent>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<ICustomerComponent, CustomerComponent>();
builder.Services.AddTransient<IDocumentComponent, DocumentComponent>();
builder.Services.AddTransient<ILinkComponent, LinkComponent>();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));


builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration.GetSection("Constants:SecurityKey").Value))
                };
            });

var app = builder.Build();

await using var scope = app.Services.CreateAsyncScope();
using var db = scope.ServiceProvider.GetService<PorscheDbContext>();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
await db.Database.MigrateAsync();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAllOrigins");

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Porsche API V1");
});

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { ShowStatusCode = true });

app.MapControllers();  

app.Run();