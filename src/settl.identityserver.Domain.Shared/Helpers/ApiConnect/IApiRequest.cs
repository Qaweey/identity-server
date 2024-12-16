using RestSharp;
using System.Threading.Tasks;

namespace settl.identityserver.Domain.Shared.Helpers.ApiConnect
{
    public interface IApiRequest<TRest>
    {
        Task<IRestResponse<TRest>> MakeRequestAsync(object postdata, string url, Method method);
        
    }
}
