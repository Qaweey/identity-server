using settl.identityserver.Appication.Contracts.RepositoryInterfaces;
using settl.identityserver.Application.Contracts.DTO.Idempotent;
using settl.identityserver.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Services
{
    public class IdempotentService
    {
        private readonly IGenericRepository<Idempotent> _repository;

        public IdempotentService(IGenericRepository<Idempotent> idempotentRepository)
        {
            _repository = idempotentRepository;
        }

        public async Task<IdempotentDTO> GetByRequestId(string idempotentRequestId)
        {
            var idempotent = await _repository.Get(x => x.RequestID == idempotentRequestId);

            return new IdempotentDTO();
            //return DataMapper.Map<IdempotentDTO, Idempotent>(idempotent);
        }

        public async Task<bool> Create(IdempotentDTO IdempotentDTO)
        {
            try
            {
                //var entity = AutoMapping.Map<TblIdempotent, IdempotentDTO>(IdempotentDTO);
                //await _repository.Create(entity);
                await _repository.Save();

                return true;
            }
            catch (Exception)
            {
                //logger
                return false;
            }
        }
    }
}