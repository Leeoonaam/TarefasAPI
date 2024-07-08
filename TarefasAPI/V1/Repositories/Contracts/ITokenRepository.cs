using TarefasAPI.V1.Models;

namespace TarefasAPI.V1.Repositories.Contracts
{
    public interface ITokenRepository
    {
        void Cadastrar(Token token);

        // Key-Value
        Token Obter(string refreshToken);

        void Atualizar(Token token);
    }
}
