using System.Threading.Tasks;

namespace Authentication.Usecases
{
    public interface IHandler<TIn, TOut> where TIn : Validatable
    {
        Task<Result<TOut>> Handle( TIn model );
    }
}
