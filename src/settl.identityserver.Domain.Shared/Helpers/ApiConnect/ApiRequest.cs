using Newtonsoft.Json;
using RestSharp;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace settl.identityserver.Domain.Shared.Helpers.ApiConnect
{
    public class ApiRequest<TRest> : IApiRequest<TRest>
    {
        protected RestClient _client;
        protected RestRequest _request;
        protected string _baseurl { get; set; }
        protected string _baseapp { get; set; }
        protected string _contentType { get; set; }
        protected string _acceptType { get; set; }

        protected virtual void InitializeRequest()
        {
            _client = new RestClient();
            _client.Timeout = -1;
            _request = new RestRequest();
            _request.AddHeader("Content-Type", _contentType);
        }

        protected void AddHeader(string key, string value)
        {
            if (_request != null)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    _request.AddHeader(key, value);
                }
            }
        }

        protected string SerializeData(object postdata)
        {
            var resp = "";
            switch (_contentType)
            {
                case "application/json":
                    resp = JsonConvert.SerializeObject(postdata);
                    break;

                case "application/x-www-form-urlencoded":
                    var properties = from p in postdata.GetType().GetProperties()
                                     where p.GetValue(postdata, null) != null
                                     select p.Name + "=" + p.GetValue(postdata, null).ToString();

                    resp = String.Join("&", properties.ToArray());
                    break;
            }
            return resp;
        }

        public async Task<IRestResponse<TRest>> MakeRequestAsync(object postdata, string url, Method method)
        {
            _client.BaseUrl = string.IsNullOrEmpty(_baseapp) ? new Uri(_baseurl + url) : new Uri(_baseurl + _baseapp + url);
            Log.Information($"API Call Url:{_client.BaseUrl} Method:{method} Data:{JsonHelper.SerializeObject(postdata)} Headers: {_client.DefaultParameters[_client.DefaultParameters.Count - 1].Name}:-{_client.DefaultParameters[_client.DefaultParameters.Count - 1].Value}");
            try
            {
                switch (method)
                {
                    case Method.GET:
                        _request.Method = Method.GET;
                        _request.AddParameter(_acceptType, postdata, ParameterType.QueryStringWithoutEncode);
                        break;

                    case Method.POST:
                        _request.Method = Method.POST;
                        _request.AddParameter(_acceptType, postdata, ParameterType.RequestBody);
                        break;

                    case Method.PUT:
                        _request.Method = Method.POST;
                        break;

                    case Method.DELETE:
                        _request.Method = Method.POST;
                        break;

                    default:
                        break;
                }

                var response = await _client.ExecuteAsync<TRest>(_request);
                return response;
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
        }

       
    }
}