using TarefasAPI.Data;
using TarefasAPI.V1.Models;
using TarefasAPI.V1.Repositories.Contracts;

namespace TarefasAPI.V1.Repositories
{
    public class TarefaRepository : ITarefaRepository // Implementa a interface ITarefaRepository na classe TarefaRepository.
    {
        // Declara uma variável privada somente leitura do tipo TarefasContext.
        private readonly TarefasContext _banco;

        // Construtor da classe que recebe um TarefasContext como parâmetro.
        public TarefaRepository(TarefasContext banco)
        {
            _banco = banco; // Inicializa a variável _banco com o valor passado ao construtor.
        }

        // Método que restaura tarefas com base no usuário e na data da última sincronização.
        public List<Tarefa> Restauracao(ApplicationUser usuario, DateTime dataUltSincronizacao)
        {
            // Cria uma consulta para obter as tarefas do usuário específico.
            var query = _banco.Tarefas.Where(a => a.UsuarioId == usuario.Id).AsQueryable();

            // Verifica se a data da última sincronização não é nula.
            if (dataUltSincronizacao != null)
            {
                // Filtra as tarefas criadas ou atualizadas desde a última sincronização.
                query.Where(a => a.Criado >= dataUltSincronizacao || a.Atualizado >= dataUltSincronizacao);
            }

            // Executa a consulta e retorna a lista de tarefas.
            return query.ToList();
        }

        // Método que sincroniza uma lista de tarefas.
        public List<Tarefa> Sincronizacao(List<Tarefa> tarefas)
        {
            // Cadastro
            var novastarefas = tarefas.Where(a => a.IdTarefaAPI == 0).ToList(); // Seleciona as tarefas que não têm um ID definido (novas tarefas).
            var TarefasExcluidasAtualizadas = tarefas.Where(a => a.IdTarefaAPI != 0).ToList(); // Seleciona as tarefas que já têm um ID definido (tarefas existentes).

            // Verifica se há novas tarefas para cadastrar.
            if (novastarefas.Count() > 0)
            {
                // Itera sobre as novas tarefas.
                foreach (var tarefa in novastarefas)
                {
                    // Adiciona cada nova tarefa ao contexto do banco de dados.
                    _banco.Tarefas.Add(tarefa);
                }
            }

            // Atualizacao
            // Verifica se há tarefas para atualizar.
            if (TarefasExcluidasAtualizadas.Count() > 0)
            {
                // Itera sobre as tarefas existentes.
                foreach (var tarefa in TarefasExcluidasAtualizadas)
                {
                    // Atualiza cada tarefa existente no contexto do banco de dados.
                    _banco.Tarefas.Update(tarefa);
                }
            }

            // Salva as alterações (cadastro ou atualizacao) no banco de dados.
            _banco.SaveChanges();

            // Retorna a lista de novas tarefas.
            return novastarefas.ToList();
        }
    }
}
