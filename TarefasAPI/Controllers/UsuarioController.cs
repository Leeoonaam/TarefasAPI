using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding; 
using System.Text;
using TarefasAPI.Models; 
using TarefasAPI.Repositories.Contracts; 

namespace TarefasAPI.Controllers 
{
    [Route("api/[controller]")] // Define a rota base para o controlador.
    [ApiController] // Indica que este controlador é um controlador de API.
    // Herda de ControllerBase, que é a classe base para controladores de API.
    public class UsuarioController : ControllerBase 
    {
        private readonly IUsuarioRepository _usuarioRepository; // Declara uma variável privada somente leitura do tipo IUsuarioRepository.
        private readonly SignInManager<ApplicationUser> _signInManager; // Declara uma variável privada somente leitura do tipo SignInManager<ApplicationUser>.
        private readonly UserManager<ApplicationUser> _userManager; // Declara uma variável privada somente leitura do tipo UserManager<ApplicationUser>.

        public UsuarioController(IUsuarioRepository usuarioRepository, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _usuarioRepository = usuarioRepository; // Inicializa a variável _usuarioRepository com o valor passado ao construtor.
            _signInManager = signInManager; // Inicializa a variável _signInManager com o valor passado ao construtor.
            _userManager = userManager; // Inicializa a variável _userManager com o valor passado ao construtor.
        }

        [HttpPost("login")] // Define a rota para o método de login.
        // Define a ação de login, que aceita um objeto UsuarioDTO no corpo da requisição.
        public ActionResult Login([FromBody] UsuarioDTO usuarioDTO) 
        {
            ModelState.Remove("Nome"); // Remove a validação do campo Nome do estado do modelo.
            ModelState.Remove("ConfirmacaoSenha"); // Remove a validação do campo ConfirmacaoSenha do estado do modelo.

            // Verifica se o estado do modelo é válido.
            if (ModelState.IsValid) 
            {
                // Obtém o usuário do repositório com base no email e senha fornecidos.
                ApplicationUser usuario = _usuarioRepository.Obter(usuarioDTO.Email, usuarioDTO.Senha);

                // Verifica se o usuário foi encontrado.
                if (usuario != null) 
                {
                    // Realiza o login do usuário.
                    _signInManager.SignInAsync(usuario, false);
                    // Retorna um status 200 OK.
                    return Ok(); 
                }
                else
                {
                    // Retorna um status 404 Not Found com a mensagem de erro.
                    return NotFound("Usuário não localizado!"); 
                }
            }
            else
            {
                // Retorna um status 422 Unprocessable Entity com o estado do modelo.
                return UnprocessableEntity(ModelState); 
            }
        }

        [HttpPost("")] // Define a rota para o método de cadastro vazio que é adotado para padrão.
        // Define a ação de cadastro, que aceita um objeto UsuarioDTO no corpo da requisição.
        public ActionResult Cadastrar([FromBody] UsuarioDTO usuarioDTO) 
        {
            // Verifica se o estado do modelo é válido.
            if (ModelState.IsValid) 
            {
                ApplicationUser usuario = new ApplicationUser(); // Cria uma nova instância de ApplicationUser.
                usuario.FullName = usuarioDTO.Nome; // Define o nome completo do usuário.
                usuario.UserName = usuarioDTO.Email; // Define o user do usuario
                usuario.Email = usuarioDTO.Email; // Define o email do usuário.

                // Cria o usuário de forma assíncrona e obtém o resultado.
                var resultado = _userManager.CreateAsync(usuario, usuarioDTO.Senha).Result; 

                // Verifica se a criação do usuário não foi bem-sucedida.
                if (!resultado.Succeeded) 
                {
                    // Cria uma lista para armazenar as mensagens de erro.
                    List<String> erros = new List<string>();

                    // Loop sobre os erros ocorridos durante a criação do usuário.
                    foreach (var erro in resultado.Errors) 
                    {
                        // Adiciona a descrição do erro à lista de erros.
                        erros.Add(erro.Description); 
                    }

                    // Retorna um status 422 Unprocessable Entity com a lista de erros.
                    return UnprocessableEntity(erros); 
                }
                else
                {
                    // Retorna um status 200 OK com o usuário criado.
                    return Ok(usuario); 
                }
            }
            else
            {
                // Retorna um status 422 Unprocessable Entity com o estado do modelo.
                return UnprocessableEntity(ModelState); 
            }
        }
    }
}
