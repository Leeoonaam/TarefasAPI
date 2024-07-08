using Microsoft.EntityFrameworkCore;
using TarefasAPI.Data;
using TarefasAPI.Models;
using TarefasAPI.Repositories;
using TarefasAPI.Repositories.Contracts;
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
builder.Services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_2)
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; // adiciona a op��o de loop no retorno do json para trativa de tarefas em um unico id de usuario
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
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseHttpsRedirection();
app.UseStatusCodePages();
app.UseAuthentication(); // Certifique-se de que a autentica��o � usada antes da autoriza��o
app.UseAuthorization();

app.MapControllers();

app.Run();
