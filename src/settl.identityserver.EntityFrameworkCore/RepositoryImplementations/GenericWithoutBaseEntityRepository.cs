using Microsoft.EntityFrameworkCore;
using settl.identityserver.Application.Contracts.RepositoryInterfaces;
using settl.identityserver.EntityFrameworkCore.AppDbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace settl.identityserver.EntityFrameworkCore.RepositoryImplementations
{
    public class GenericWithoutBaseEntityRepository<TEntity> : IGenericWithoutBaseEntityRepository<TEntity> where TEntity : class
    {
        private readonly ApplicationDbContext _dbContext;

        public GenericWithoutBaseEntityRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<TEntity> GetAll()
        {
            return _dbContext.Set<TEntity>().AsNoTracking();
        }

        public async Task<TEntity> Get(Expression<Func<TEntity, bool>> where)
        {
            return await _dbContext.Set<TEntity>().FirstOrDefaultAsync(where);
        }

        public async Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>> where)
        {
            return await _dbContext.Set<TEntity>()
                .Where(where).ToListAsync();
        }

        public async Task Create(TEntity entity)
        {
            await _dbContext.Set<TEntity>().AddAsync(entity);
        }

        public async Task Add(TEntity entity)
        {
            await _dbContext.Set<TEntity>().AddAsync(entity);
        }

        public async Task AddRange(List<TEntity> entities)
        {
            await _dbContext.Set<TEntity>().AddRangeAsync(entities);
        }

        public void Update(TEntity entity)
        {
            _dbContext.Attach(entity);
            _dbContext.Update(entity);
        }

        public void Delete(TEntity entity)
        {
            _dbContext.Set<TEntity>().Remove(entity);
        }

        public IQueryable<TEntity> Query()
        {
            return _dbContext.Set<TEntity>();
        }

        public IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> where)
        {
            return _dbContext.Set<TEntity>()
                .Where(where);
        }

        public async Task<bool> Save()
        {
            return await ((DbContext)_dbContext).SaveChangesAsync(default) >= 0;
        }

        public void DeleteRange(IEnumerable<TEntity> entities)
        {
            _dbContext.Set<TEntity>().RemoveRange(entities);
        }

        public void UpdateRange(List<TEntity> entities)
        {
            _dbContext.Set<TEntity>().UpdateRange(entities);
        }
    }
}