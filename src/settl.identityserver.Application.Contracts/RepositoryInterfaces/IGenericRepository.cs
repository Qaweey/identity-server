using settl.identityserver.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace settl.identityserver.Appication.Contracts.RepositoryInterfaces
{
    public interface IGenericRepository<TEntity>
        where TEntity : Entity
    {
        IQueryable<TEntity> GetAll();

        IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> where);

        Task<TEntity> Get(Expression<Func<TEntity, bool>> where);

        Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>> where);

        IQueryable<TEntity> Query();

        Task Create(TEntity entity);

        Task Add(TEntity entity);

        Task AddRange(List<TEntity> entity);

        void Update(TEntity entity);

        void UpdateRange(List<TEntity> entities);

        void Delete(TEntity entity);

        void DeleteRange(IEnumerable<TEntity> entities);

        void SoftDelete(TEntity entity);

        Task<bool> Save();
    }
}