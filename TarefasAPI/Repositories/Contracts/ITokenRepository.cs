using TarefasAPI.Models;

namespace TarefasAPI.Repositories.Contracts
{
    public interface ITokenRepository
    {
        void Cadastrar(Token token);

        // Key-Value
        Token Obter(string refreshToken);

        void Atualizar(Token token);
    }
}
