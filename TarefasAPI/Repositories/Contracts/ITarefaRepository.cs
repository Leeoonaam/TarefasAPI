using TarefasAPI.Models;

namespace TarefasAPI.Repositories.Contracts
{
    public interface ITarefaRepository
    {
        // pega as tarefas novas e vai enciar para a API armazenar e fazer o backup | uma lista para que api receba todas as operações(cadastro, atualizacao e etc) em uma unica requisição
        List<Tarefa> Sicronizacao(List<Tarefa> tarefas); 

        //Backup      
        List<Tarefa> Restauracao(ApplicationUser usuario, DateTime dataUltSicronizacao);
    }
}
