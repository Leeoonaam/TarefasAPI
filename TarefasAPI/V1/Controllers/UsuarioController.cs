using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TarefasAPI.Migrations;
using TarefasAPI.V1.Models;
using TarefasAPI.V1.Repositories.Contracts;

namespace TarefasAPI.V1.Controllers
{
    [Route("api/[controller]")] // Define a rota base para o controlador.
    [ApiController] // Indica que este controlador é um controlador de API.
    [ApiVersion("1.0")]
    // Herda de ControllerBase, que é a classe base para controladores de API.
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository; // Declara uma variável privada somente leitura do tipo IUsuarioRepository.
        private readonly SignInManager<ApplicationUser> _signInManager; // Declara uma variável privada somente leitura do tipo SignInManager<ApplicationUser>.
        private readonly UserManager<ApplicationUser> _userManager; // Declara uma variável privada somente leitura do tipo UserManager<ApplicationUser>.
        private readonly ITokenRepository _tokenRepository; //Declara uma variável privada somente leitura do tipo ItokenRepository.

        public UsuarioController(IUsuarioRepository usuarioRepository, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ITokenRepository tokenRepository)
        {
            _usuarioRepository = usuarioRepository; // Inicializa a variável _usuarioRepository com o valor passado ao construtor.
            _signInManager = signInManager; // Inicializa a variável _signInManager com o valor passado ao construtor.
            _userManager = userManager; // Inicializa a variável _userManager com o valor passado ao construtor.
            _tokenRepository = tokenRepository; //Inicializa a variável _tokenRepository com o valor passado ao construtor.
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
                    // REMOVE PARA USAR O JTW
                    // Realiza o login do usuário identity.
                    //_signInManager.SignInAsync(usuario, false);

                    // Chama o metodo para gerar o token
                    return GerarToken(usuario);
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

        [HttpPost("Renovar")]
        public ActionResult Renovar([FromBody] TokenDTO tokenDTO)
        {
            var refreshTokenDB = _tokenRepository.Obter(tokenDTO.RefreshToken);

            if (refreshTokenDB == null)
            {
                return NotFound();
            }

            //refreshtoken antigo - atualizar(desativar refresh)
            refreshTokenDB.Atualizado = DateTime.Now;
            refreshTokenDB.Utilizado = true;
            _tokenRepository.Atualizar(refreshTokenDB);

            //gerar um novo/refresh tokn e salvar
            // Chama o metodo para gerar o token
            var usuario = _usuarioRepository.Obter(refreshTokenDB.UsuarioId);

            return GerarToken(usuario);
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
                    List<string> erros = new List<string>();

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


        /// <summary>
        /// Método que gera um token JWT para um usuário específico
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private TokenDTO BuildToken(ApplicationUser usuario)
        {
            // Definição das claims do token, que são declarações sobre o usuário (email e id do usuário)
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id)
            };

            // Chave usada para criptografia do token (deve ter pelo menos 32 caracteres)
            // Recomenda-se armazenar esta chave no arquivo appsettings.json para maior segurança e flexibilidade
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("chave-api-jwt-minhas-tarefas-e-com-no-minimo-32-caracteres"));
            var sign = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var exp = DateTime.UtcNow.AddHours(1);

            // Criação do token JWT com os parâmetros especificados (issuer, audience, claims, expiração e assinatura)
            JwtSecurityToken token = new JwtSecurityToken(
                issuer: null, //dominio
                audience: null, //quem recebe o dominio
                claims: claims,
                expires: exp, // expiração de uma hora
                signingCredentials: sign
            );
            // Geração do token JWT como uma string
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            var refreshToken = Guid.NewGuid().ToString();
            var expRefreshToken = DateTime.UtcNow.AddHours(2);


            var tokenDTO = new TokenDTO { Token = tokenString, Expiration = exp, ExpirationRefreshToken = expRefreshToken, RefreshToken = refreshToken };




            // Retorno de um objeto anônimo contendo o token e a data de expiração
            return tokenDTO;
        }

        /// <summary>
        /// Metodo para gerar token
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private ActionResult GerarToken(ApplicationUser usuario)
        {
            var token = BuildToken(usuario);

            //Gera uma noca e salva o token no banco
            var tokenModel = new Models.Token()
            {
                RefreshToken = token.RefreshToken,
                ExpirationToken = token.Expiration,
                ExpirationRefreshToken = token.ExpirationRefreshToken,
                Usuario = usuario,
                Criado = DateTime.Now,
                Utilizado = false
            };
            _tokenRepository.Cadastrar(tokenModel);

            return Ok(token);
        }
    }
}
