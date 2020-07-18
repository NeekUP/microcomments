using Authentication.Usecases;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Authentication.Infrastructure.DataAccess
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        internal UserManagementDBContext context;
        internal DbSet<TEntity> dbSet;

        public GenericRepository( UserManagementDBContext context )
        {
            this.context = context;
            this.dbSet = context.Set<TEntity>();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string[] includeProperties = null,
            bool AsNoTracking = false )
        {
            IQueryable<TEntity> query = dbSet;

            if ( filter != null )
            {
                query = query.Where( filter );
            }

            if ( includeProperties != null )
            {
                foreach ( var includeProperty in includeProperties )
                {
                    query = query.Include( includeProperty );
                }
            }

            if( AsNoTracking )
            {
                query = query.AsNoTracking();
            }

            if ( orderBy != null )
            {
                return await orderBy( query ).ToListAsync();
            }
            else
            {
                return query.ToList();
            }
        }

        public virtual async Task<TEntity> GetByIDAsync( object id )
        {
            return await dbSet.FindAsync( id );
        }

        public virtual void Insert( TEntity entity )
        {
            dbSet.Add( entity );
        }

        public virtual void Delete( object id )
        {
            TEntity entityToDelete = dbSet.Find( id );
            Delete( entityToDelete );
        }

        public virtual void Delete( TEntity entityToDelete )
        {
            if ( context.Entry( entityToDelete ).State == EntityState.Detached )
            {
                dbSet.Attach( entityToDelete );
            }
            dbSet.Remove( entityToDelete );
        }

        public virtual void Update( TEntity entityToUpdate )
        {
            dbSet.Attach( entityToUpdate );
            context.Entry( entityToUpdate ).State = EntityState.Modified;
        }
    }
}
