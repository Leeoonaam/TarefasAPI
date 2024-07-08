using Microsoft.AspNetCore.Identity; // Importa o namespace necessário para trabalhar com identidade e gerenciamento de usuários.
using System.Text; // Importa o namespace necessário para manipulação de strings.
using TarefasAPI.Models; // Importa o namespace onde está definido o modelo ApplicationUser.
using TarefasAPI.Repositories.Contracts; // Importa o namespace onde está definida a interface IUsuarioRepository.

namespace TarefasAPI.Repositories 
{
    public class UsuarioRepository : IUsuarioRepository // Implementa a interface IUsuarioRepository na classe UsuarioRepository.
    {
        // Declara uma variável privada somente leitura do tipo UserManager<ApplicationUser>.
        private readonly UserManager<ApplicationUser> _userManager;

        // Construtor da classe que recebe um UserManager<ApplicationUser> como parâmetro.
        public UsuarioRepository(UserManager<ApplicationUser> userManager) 
        {
            // Inicializa a variável _userManager com o valor passado ao construtor.
            _userManager = userManager; 
        }

        // Método que obtém um usuário com base no email e senha fornecidos.
        public ApplicationUser Obter(string email, string senha) 
        {
            var usuario = _userManager.FindByEmailAsync(email).Result; // Busca o usuário pelo email de forma assíncrona e obtém o resultado.
            // Verifica se a senha está correta de forma assíncrona e obtém o resultado.
            if (_userManager.CheckPasswordAsync(usuario, senha).Result) 
            {
                // Retorna o usuário se a senha estiver correta.
                return usuario; 
            }
            else
            {
                // Lança uma exceção se a senha estiver incorreta.
                throw new Exception("Usuário não encontrado!"); 
            }
        }

        // Método que cadastra um novo usuário com a senha fornecida.
        public void Cadastrar(ApplicationUser usuario, string senha) 
        {
            var result = _userManager.CreateAsync(usuario, senha).Result; // Cria o usuário de forma assíncrona e obtém o resultado.
            // Verifica se a criação do usuário não foi bem-sucedida.
            if (!result.Succeeded) 
            {
                StringBuilder sb = new StringBuilder(); // Inicializa um StringBuilder para coletar mensagens de erro.
                // Loop para iterar sobre os erros ocorridos durante a criação do usuário.
                foreach (var erro in result.Errors) 
                {
                    sb.Append(erro.Description); // Adiciona a descrição do erro ao StringBuilder.
                }

                // Lança uma exceção com todas as mensagens de erro.
                throw new Exception($"Usuário não cadastrado! Erro: {sb.ToString()}"); 
            }
        }

        public ApplicationUser Obter(string id)
        {
            return _userManager.FindByIdAsync(id).Result; // Busca o usuário pelo email de forma assíncrona e obtém o resultado.
        }
    }
}
