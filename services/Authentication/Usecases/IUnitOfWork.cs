using System;
using System.Threading.Tasks;

namespace Authentication.Usecases
{
    public interface IUnitOfWork : IDisposable
    {
        ITokensRepository Tokens { get; }
        IUsersRepository Users { get; }

        Task SaveAsync();
    }
}