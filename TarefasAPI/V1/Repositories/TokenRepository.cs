using TarefasAPI.Data;
using TarefasAPI.V1.Models;
using TarefasAPI.V1.Repositories.Contracts;

namespace TarefasAPI.V1.Repositories
{
    public class TokenRepository : ITokenRepository
    {
        private readonly TarefasContext _banco;

        public TokenRepository(TarefasContext banco)
        {
            _banco = banco;
        }

        public Token Obter(string refreshToken)
        {
            return _banco.Token.FirstOrDefault(a => a.RefreshToken == refreshToken && a.Utilizado == false);
        }

        public void Cadastrar(Token token)
        {
            _banco.Token.Add(token);
            _banco.SaveChanges();
        }

        public void Atualizar(Token token)
        {
            _banco.Token.Update(token);
            _banco.SaveChanges();
        }

    }
}
