using Newtonsoft.Json;
using RestSharp;
using Serilog;
using settl.identityserver.Application.Contracts.DTO.Consumer;
using settl.identityserver.Application.Contracts.RepositoryInterfaces;
using settl.identityserver.Domain.Shared.Enums;
using settl.identityserver.Domain.Shared.Helpers.ApiConnect;
using System;
using System.Net;
using System.Threading.Tasks;

namespace settl.identityserver.EntityFrameworkCore.RepositoryImplementations
{
    public class ConsumerRepository<T> : ApiRequest<T>, IConsumerRepository<T>
    {
        public void Connect(string token)
        {
            InitializeRequest();
            _baseurl = Constants.CONSUMER_URL;
            _contentType = "application/json";
            _acceptType = "application/json";
            _client.AddDefaultHeader("X-Settl-Api-Token", Constants.SETTL_API_TOKEN);
            _client.AddDefaultHeader("x-RequestID", Guid.NewGuid().ToString());
            _client.AddDefaultHeader("Authorization", $"Bearer {token}");
        }

        public async Task<(CreateConsumerResponseDTO, HttpStatusCode)> PostCreateConsumerAsync(object data, string url, Method method = Method.POST)
        {
            try
            {
                _request.RequestFormat = DataFormat.Json;
                var json = SerializeData(data);
                var payload = JsonConvert.DeserializeObject<CreateConsumerDTO>(json);
                _request.AddJsonBody(new { payload.Email, payload.FirstName, payload.LastName, PhoneNo = payload.Phone, payload.UserName, UsedReferralCode = payload.ReferralCode, payload.Gender });
                var apiresp = await MakeRequestAsync(data, url, method);
                Log.Information($"Consumer response - {apiresp.Content} | Status code - {apiresp.StatusCode}");
                var responseData = JsonConvert.DeserializeObject<CreateConsumerResponseDTO>(apiresp.Content);
                return (responseData, apiresp.StatusCode);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<(ConsumerProfileResponseDTO, HttpStatusCode)> GetProfile(object data, string url, Method method = Method.GET)
        {
            var response = await MakeRequestAsync(data, url, method);

            if (response.StatusCode != HttpStatusCode.OK) throw new Exception($"{response.StatusCode} {response.Content}");

            var responseData = JsonConvert.DeserializeObject<ConsumerProfileResponseDTO>(response.Content);
            return (responseData, response.StatusCode);
        }
    }
}