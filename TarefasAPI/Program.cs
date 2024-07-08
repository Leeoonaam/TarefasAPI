using Microsoft.EntityFrameworkCore;
using TarefasAPI.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.OpenApi.Models;
using TarefasAPI.V1.Models;
using TarefasAPI.V1.Repositories;
using TarefasAPI.V1.Repositories.Contracts;
using Microsoft.Extensions.PlatformAbstractions;
using TarefasAPI.V1.Helpers.Swagger;
using Swashbuckle.AspNetCore.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configura��o para suprimir o filtro de modelo inv�lido ao tratamento de erros
builder.Services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

// Configura��o de banco de dados local no projeto
builder.Services.AddDbContext<TarefasContext>(options =>
{
    options.UseSqlite("Data Source=Data\\MinhasTarefas.db");
});

// Configura��o de depend�ncia dos reposit�rios criados
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ITarefaRepository, TarefaRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();

// Configura��o para adicionar o MVC com compatibilidade 
builder.Services.AddMvc(config => { 
    //configura��o para utilizar XML
    config.ReturnHttpNotAcceptable = true;
    config.InputFormatters.Add(new XmlSerializerInputFormatter(config)); // XML de entrada
    config.OutputFormatters.Add(new XmlSerializerOutputFormatter()); // XML de saida
})
    .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_2)
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; // adiciona a op��o de loop no retorno do json para trativa de tarefas em um unico id de usuario
    });

// adiciona configura��o de versionamento da api
builder.Services.AddApiVersioning(cfg =>
{
    cfg.ReportApiVersions = true; // retorna no cabe�alho uma lista de vers�o disponiveis para que o usuario possa migrar de acordo com sua documenta��o
    //cfg.ApiVersionReader = new HeaderApiVersionReader("api-version"); // habilita a possibilidade de realizar a consulta utilizando o cabe�alho
    cfg.AssumeDefaultVersionWhenUnspecified = true; // configura��o para direciona o usuario para uma vers�o padr�o, quando nao especifica na URL
    cfg.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0); // definindo a vers�o padr�o
    //cfg.ApiVersionReader = new MediaTypeApiVersionReader();
});

// configura��o do swagger
builder.Services.AddSwaggerGen(c =>
{
    // Configura��o para Chave de API
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header, // cabe�alho
        Type = SecuritySchemeType.ApiKey, // valor na tela
        Name = "Authorization",
        Description = "Insira o Json Web Token (JWT) para realizar autenticar (realize o login para captura do Token)"
    });

    var securityRequirement = new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                In = ParameterLocation.Header,
                Scheme = "Bearer",
                Name = "Authorization",
                BearerFormat = "JWT"
            },
            new List<string>()
        }
    };

    c.AddSecurityRequirement(securityRequirement);

    // configura��o para resolver o conflito de URL ou rotas, vai pegar o primeiro e deconsiderar os demais
    c.ResolveConflictingActions(ApiDescription => ApiDescription.First());
    //primeira vers�o | classe info para informa��es da API de sua escolha
    c.SwaggerDoc("v1.0", new OpenApiInfo { Title = "API - Minhas Tarefas - V1.0", Version = "v1.0" });

    var CaminhoProjeto = PlatformServices.Default.Application.ApplicationBasePath;
    var NomeProjeto = $"{PlatformServices.Default.Application.ApplicationName}.xml";
    var CaminhoArquivoXMLComentario = Path.Combine(CaminhoProjeto,NomeProjeto);

    // Localiza��o do arquivo XML
    var xmlFile = $"{System.AppDomain.CurrentDomain.BaseDirectory}TarefasAPI.xml";
    if (File.Exists(xmlFile))
    {
        c.IncludeXmlComments(CaminhoArquivoXMLComentario);
    }

    // Define um predicado que determina se uma API espec�fica (apiDesc) deve ser inclu�da em um documento de API (docName).
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        var actionApiVersionModel = apiDesc.ActionDescriptor?.GetApiVersion();
        // significaria que esta a��o n�o � versionada e deve ser inclu�da em todos os lugares
        if (actionApiVersionModel == null)
        {
            return true;
        }
        if (actionApiVersionModel.DeclaredApiVersions.Any())
        {
            return actionApiVersionModel.DeclaredApiVersions.Any(V => $"v{V.ToString()}" == docName);
        }
        return actionApiVersionModel.ImplementedApiVersions.Any(V => $"v{V.ToString()}" == docName);
    });

    //filtragem por vers�o no swagger
    c.OperationFilter<ApiVersionOperationFilter>();

});

//configura o [Authorize]
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opt =>
{
    //valida os paremtros do token se � valido
    opt.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("chave-api-jwt-minhas-tarefas-e-com-no-minimo-32-caracteres"))
    };
});

// configura��o de autoriza��o
builder.Services.AddAuthorization(auth =>
{
    auth.AddPolicy("Bearer",new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme) // esquema de autentica��o
        .RequireAuthenticatedUser() //verifica o usuario
        .Build());
});


// Identity
builder.Services.AddIdentity <ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true; // Configura��es de op��es para Identity
})
.AddEntityFrameworkStores<TarefasContext>()// Adiciona o armazenamento do Entity Framework para Identity usando o contexto do TarefasContext
.AddDefaultTokenProviders(); // habilita o uso de token 

// configura��o para redirecionamento para tela caso n�o esteja logado
// Apresenta a mensagem correta de erro para 401: n�o auturizado por n�o estar logado, caso ocorra o erro 404 que por padr�o o erro envia pra uma tela n�o existente
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "V1.0");
    });
}

app.UseAuthentication();
app.UseHttpsRedirection();
app.UseStatusCodePages();
app.UseAuthentication(); // Certifique-se de que a autentica��o � usada antes da autoriza��o
app.UseAuthorization();

app.MapControllers();

app.Run();
