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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuração para suprimir o filtro de modelo inválido ao tratamento de erros
builder.Services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

// Configuração de banco de dados local no projeto
builder.Services.AddDbContext<TarefasContext>(options =>
{
    options.UseSqlite("Data Source=Data\\MinhasTarefas.db");
});

// Configuração de dependência dos repositórios criados
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ITarefaRepository, TarefaRepository>();

// Configuração para adicionar o MVC com compatibilidade 
builder.Services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_2)
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; // adiciona a opção de loop no retorno do json para trativa de tarefas em um unico id de usuario
    });

// Identity
builder.Services.AddIdentity <ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true; // Configurações de opções para Identity
})
.AddEntityFrameworkStores<TarefasContext>(); // Adiciona o armazenamento do Entity Framework para Identity usando o contexto do TarefasContext

// configuração para redirecionamento para tela caso não esteja logado
// Apresenta a mensagem correta de erro para 401: não auturizado por não estar logado, caso ocorra o erro 404 que por padrão o erro envia pra uma tela não existente
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

app.UseHttpsRedirection();
app.UseStatusCodePages();
app.UseAuthentication(); // Certifique-se de que a autenticação é usada antes da autorização
app.UseAuthorization();

app.MapControllers();

app.Run();
