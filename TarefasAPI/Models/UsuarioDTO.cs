using System.ComponentModel.DataAnnotations;

namespace TarefasAPI.Models
{
    public class UsuarioDTO
    {
        [Required] //campo obrigatorio
        public string Nome { get; set; }
        [Required]
        [EmailAddress] //[EmailAddress(ErrorMessage = "E-mail inválido!")] - formato para descrever alguma mensagem de erro
        public string Email { get; set; }
        [Required]
        public string Senha { get; set; }
        [Required]
        [Compare("Senha")] //comparar valor do campo com outro
        public string ConfirmacaoSenha { get; set; }

    }
}
