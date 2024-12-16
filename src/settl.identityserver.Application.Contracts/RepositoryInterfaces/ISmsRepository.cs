using RestSharp;
using settl.identityserver.Application.Contracts.DTO.SMS;
using settl.identityserver.Domain.Shared.Helpers.ApiConnect;
using System.Net;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Contracts.RepositoryInterfaces
{
    public interface ISmsRepository<T> : IApiRequest<T>
    {
        Task<(BaseSettlApiDTO, HttpStatusCode)> PostSMSAsync(object data, string url, Method method = Method.POST);
    }
}