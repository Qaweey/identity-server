using RestSharp;
using settl.identityserver.Application.Contracts.DTO.Consumer;
using settl.identityserver.Domain.Shared.Helpers.ApiConnect;
using System.Net;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Contracts.RepositoryInterfaces
{
    public interface IConsumerRepository<T> : IApiRequest<T>
    {
        Task<(CreateConsumerResponseDTO, HttpStatusCode)> PostCreateConsumerAsync(object data, string url, Method method = Method.POST);

        Task<(ConsumerProfileResponseDTO, HttpStatusCode)> GetProfile(object data, string url, Method method = Method.GET);
    }
}