using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace TarefasAPI.V1.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual ICollection<Tarefa> Tarefas { get; set; } //relacionamento com tarefas por meio dessa propriedade

        [ForeignKey("UsuarioId")]
        public virtual ICollection<Token> Tokens { get; set; } //relacionamento com tarefas por meio dessa propriedade
    }
}
