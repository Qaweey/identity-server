using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Contracts.RepositoryInterfaces
{
    public interface IGenericWithoutBaseEntityRepository<TEntity> where TEntity : class
    {
        Task Add(TEntity entity);

        Task AddRange(List<TEntity> entities);

        Task Create(TEntity entity);

        void Delete(TEntity entity);

        void DeleteRange(IEnumerable<TEntity> entities);

        Task<TEntity> Get(Expression<Func<TEntity, bool>> where);

        IQueryable<TEntity> GetAll();

        Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>> where);

        IQueryable<TEntity> Query();

        IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> where);

        Task<bool> Save();

        void Update(TEntity entity);

        void UpdateRange(List<TEntity> entities);
    }
}