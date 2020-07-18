using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Authentication.Usecases
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        void Delete( object id );
        void Delete( TEntity entityToDelete );
        Task<IEnumerable<TEntity>> GetAsync( 
            Expression<Func<TEntity, bool>> filter = null, 
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, 
            string[] includeProperties = null,
            bool asNoTracking = false );
        Task<TEntity> GetByIDAsync( object id );
        void Insert( TEntity entity );
        void Update( TEntity entityToUpdate );
    }
}