using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TarefasAPI.Models;
using TarefasAPI.Repositories;
using TarefasAPI.Repositories.Contracts;

namespace TarefasAPI.Controllers 
{
    [Route("api/[controller]")] // Define a rota base para o controlador.
    [ApiController] // Indica que este controlador é um controlador de API.
    // Herda de Controller, que é a classe base para controladores MVC.
    public class TarefaController : Controller
    {
        private readonly ITarefaRepository _tarefarepository; // Declara uma variável privada somente leitura do tipo ITarefaRepository.
        private readonly UserManager<ApplicationUser> _userManager; // Declara uma variável privada somente leitura do tipo UserManager<ApplicationUser>.

        public TarefaController(ITarefaRepository tarefaRepository, UserManager<ApplicationUser> userManager)
        {
            _tarefarepository = tarefaRepository; // Inicializa a variável _tarefarepository com o valor passado ao construtor.
            _userManager = userManager; // Inicializa a variável _userManager com o valor passado ao construtor.
        }

        [Authorize] // define autorazição do login para acessar
        [HttpPost("sincronizar")] // Define a rota para o método de sincronização.
        // Define a ação de sincronização, que aceita uma lista de tarefas no corpo da requisição.
        public ActionResult Sincronizar([FromBody] List<Tarefa> tarefas)
        {
            // Chama o método Sincronizacao do repositório e retorna o resultado com um status 200 OK.
            return Ok(_tarefarepository.Sincronizacao(tarefas));
        }

        [Authorize] // define autorazição do login para acessar
        [HttpGet("restaurar")] // Define a rota para o método de restauração. GET para retornar dados
        // Define a ação de restauração, que aceita uma data como parâmetro.
        public ActionResult Restaurar(DateTime data)
        {
            // Obtém o usuário atual de forma assíncrona.
            var usuario = _userManager.GetUserAsync(HttpContext.User).Result;
            // Chama o método Restauracao do repositório com o usuário e a data, e retorna o resultado com um status 200 OK.
            return Ok(_tarefarepository.Restauracao(usuario, data));
        }

        [HttpGet("Modelo")]
        // Modelo pra visualizar a estrutura para cadastrar
        public ActionResult Modelo()
        {
            return Ok(new Tarefa());
        }
    }
}
