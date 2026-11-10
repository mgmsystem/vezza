using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.HttpOverrides;
using Vezza.Api.Configuration;
using Vezza.Api.Services;
using Vezza.Api.Security;
using Vezza.Domain.CQRS.Handlers;
using Vezza.Domain.Repositories;
using Vezza.Domain.Services;
using Vezza.Infra.Context;
using Vezza.Infra.Repositories;
using Vezza.Shared.Security.Services;
using Vezza.Shared.Security.Token;

var builder = WebApplication.CreateBuilder(args);

var connectionString = GetDatabaseConnectionString(builder.Configuration);
var jwtKey = GetRequiredConfiguration(
    builder.Configuration["JwtKey"],
    "JwtKey");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
ConfigureSwaggerGen(builder);
ConfigureAuthentication(builder);
ConfigureMvc(builder);
LoadServices(builder, connectionString);

var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
    app.Urls.Add($"http://*:{port}");

// A documentação interativa não é exposta automaticamente fora do desenvolvimento.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
// No Heroku, toda requisição pública chega pelo roteador da plataforma.
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);
app.UseHttpsRedirection();
app.UseMiddleware<PublicEndpointRateLimitMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.MapFallbackToFile("/login", "index.html");
app.MapFallbackToFile("/app/{*path:nonfile}", "index.html");
app.MapGet("/", () => Results.Redirect("/app/dashboard"));
app.Run();




void LoadServices(WebApplicationBuilder builder, string connectionString)
{
    builder.Services.AddDbContext<DataContext>(opt => opt.UseMySQL(connectionString, null));
    // builder.Services.AddDbContext<DataContext>(opt => opt.UseSqlServer(connectionString));
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
    builder.Services.AddSingleton<TokenUtils>();
    builder.Services.AddSingleton<IEmailService>(sp =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        return new EmailService(
            configuration["Smtp:Host"],
            configuration.GetValue<int?>("Smtp:Port") ?? 587,
            configuration["Smtp:Username"],
            configuration["Smtp:Password"],
            configuration["Smtp:SenderEmail"],
            configuration["Smtp:SenderDisplayName"],
            configuration.GetValue<bool?>("Smtp:EnableSsl") ?? true);
    });

    builder.Services.AddTransient<IAutorizacaoRepository, AutorizacaoRepository>();

    builder.Services.AddTransient<IUsuarioRepository, UsuarioRepository>();
    builder.Services.AddTransient<UsuarioHandler, UsuarioHandler>();    

    builder.Services.AddTransient<IClienteRepository, ClienteRepository>();
    builder.Services.AddTransient<ClienteHandler, ClienteHandler>();

    builder.Services.AddTransient<IObraRepository, ObraRepository>();
    builder.Services.AddTransient<ObraHandler, ObraHandler>();

    builder.Services.AddTransient<IOrcamentoRepository, OrcamentoRepository>();
    builder.Services.AddTransient<OrcamentoHandler, OrcamentoHandler>();

    builder.Services.AddTransient<IOrcamentoItemRepository, OrcamentoItemRepository>();
    builder.Services.AddTransient<OrcamentoItemHandler, OrcamentoItemHandler>();

    builder.Services.AddTransient<IFluxoRepository, FluxoRepository>();
    builder.Services.AddTransient<FluxoHandler, FluxoHandler>();
}

void ConfigureMvc(WebApplicationBuilder builder)
{
    builder.Services
        .AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            //ModelState manual
            options.SuppressModelStateInvalidFilter = true;
        })
        .AddJsonOptions(x =>
        {
            //Serialização json não ter problema com referencia ciclica
            x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
        });
}

void ConfigureAuthentication(WebApplicationBuilder builder)
{
    var key = Encoding.ASCII.GetBytes(jwtKey);
    builder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(x =>
    {
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
}

static string GetRequiredConfiguration(string? value, string key)
{
    if (string.IsNullOrWhiteSpace(value))
        throw new InvalidOperationException(
            $"A configuração obrigatória '{key}' não foi definida. " +
            "Use variáveis de ambiente ou User Secrets no desenvolvimento.");

    return value;
}

static string GetDatabaseConnectionString(IConfiguration configuration)
{
    var configured = configuration.GetConnectionString("connectionString");
    if (!string.IsNullOrWhiteSpace(configured))
        return configured;

    var jawsDbUrl = configuration["JAWSDB_URL"];
    if (!string.IsNullOrWhiteSpace(jawsDbUrl))
        return JawsDbConnectionString.Create(
            jawsDbUrl,
            configuration["Database:SslMode"] ?? "Required",
            configuration.GetValue<int?>("Database:MaximumPoolSize") ?? 5);

    return GetRequiredConfiguration(null, "ConnectionStrings:connectionString ou JAWSDB_URL");
}

void ConfigureSwaggerGen(WebApplicationBuilder builder)
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Vezza API", Version = "v1" });

        // Configura o Swagger para usar um campo de autenticação Bearer
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Por favor, insira o token JWT com 'Bearer ' prefixo",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(document => new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecuritySchemeReference("Bearer", document),
                new List<string>()
            }
        });
    });
}

public partial class Program
{
}
