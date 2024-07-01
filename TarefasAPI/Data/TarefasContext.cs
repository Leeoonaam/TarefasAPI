using Microsoft.EntityFrameworkCore;
using TarefasAPI.Models;

namespace TarefasAPI.Data
{
    public class TarefasContext : DbContext
    {
        public TarefasContext(DbContextOptions<TarefasContext>options) : base(options) 
        { 

        }

        public DbSet<Tarefa> Tarefas { get; set; }
    }
}
