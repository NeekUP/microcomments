using System.Threading.Tasks;

namespace Authentication.Usecases
{
    public interface IUnitOfWork
    {
        ITokensRepository Tokens { get; }
        IUsersRepository Users { get; }

        void Dispose();
        Task SaveAsync();
    }
}