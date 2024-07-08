using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TarefasAPI.V1.Models;

namespace TarefasAPI.Data
{
    public class TarefasContext : IdentityDbContext<ApplicationUser>
    {
        public TarefasContext(DbContextOptions<TarefasContext>options) : base(options) 
        { 

        }

        public DbSet<Tarefa> Tarefas { get; set; }
        public DbSet<Token> Token { get; set; }
    }
}
