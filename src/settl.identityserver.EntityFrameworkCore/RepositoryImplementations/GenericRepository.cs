using Microsoft.EntityFrameworkCore;
using Serilog;
using settl.identityserver.Appication.Contracts.RepositoryInterfaces;
using settl.identityserver.Domain.Shared;
using settl.identityserver.Domain.Shared.Helpers;
using settl.identityserver.EntityFrameworkCore.AppDbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace settl.identityserver.EntityFrameworkCore.RepositoryImplementations
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity>
        where TEntity : Entity
    {
        private readonly ApplicationDbContext _dbContext;

        public GenericRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<TEntity> GetAll()
        {
            return _dbContext.Set<TEntity>().AsNoTracking().Where(a => !a.IsDeleted);
        }

        public async Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>> where)
        {
            return await _dbContext.Set<TEntity>()
                .Where(where).ToListAsync();
        }

        public async Task<TEntity> Get(Expression<Func<TEntity, bool>> where)
        {
            return await _dbContext.Set<TEntity>().Where(a => !a.IsDeleted).FirstOrDefaultAsync(where);
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
            entity.UpdatedOn = DateHelper.GetCurrentLocalTime();
            _dbContext.Attach(entity);
            _dbContext.Update(entity);
        }

        public void Delete(TEntity entity)
        {
            _dbContext.Set<TEntity>().Remove(entity);
        }

        public IQueryable<TEntity> Query()
        {
            return _dbContext.Set<TEntity>().Where(a => !a.IsDeleted)
                    .OrderByDescending(a => a.CreatedOn);
        }

        public IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> where)
        {
            return _dbContext.Set<TEntity>().Where(a => !a.IsDeleted)
                .Where(where)
                .OrderByDescending(a => a.CreatedOn);
        }

        public void SoftDelete(TEntity entity)
        {
            entity.IsDeleted = true;
            entity.DeletedOn = DateHelper.GetCurrentLocalTime();
            _dbContext.Set<TEntity>().Update(entity);
        }

        public async Task<bool> Save()
        {
            try
            {
                return await _dbContext.SaveChangesAsync(default) >= 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
        }

        public void DeleteRange(IEnumerable<TEntity> entities)
        {
            _dbContext.Set<TEntity>().RemoveRange(entities);
        }

        public void UpdateRange(List<TEntity> entities)
        {
            entities.ForEach(x => x.UpdatedOn = DateHelper.GetCurrentLocalTime());
            _dbContext.Set<TEntity>().UpdateRange(entities);
        }
    }
}