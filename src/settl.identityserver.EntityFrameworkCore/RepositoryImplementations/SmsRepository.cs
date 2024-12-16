using Newtonsoft.Json;
using RestSharp;
using Serilog;
using settl.identityserver.Application.Contracts.DTO.OTP;
using settl.identityserver.Application.Contracts.DTO.SMS;
using settl.identityserver.Application.Contracts.RepositoryInterfaces;
using settl.identityserver.Domain.Shared.Enums;
using settl.identityserver.Domain.Shared.Helpers.ApiConnect;
using System;
using System.Net;
using System.Threading.Tasks;

namespace settl.identityserver.EntityFrameworkCore.RepositoryImplementations
{
    public class SmsRepository<T> : ApiRequest<T>, ISmsRepository<T>
    {
        public void Connect()
        {
            _baseurl = Constants.SMSSERVICE_URL;
            _contentType = "application/json";
            _acceptType = "application/json";
            InitializeRequest();
        }

        public async Task<(BaseSettlApiDTO, HttpStatusCode)> PostSMSAsync(object data, string url, Method method = Method.POST)
        {
            try
            {
                _request.RequestFormat = DataFormat.Json;
                var json = SerializeData(data);
                var payload = JsonConvert.DeserializeObject<SMSRequest>(json);

                _request.AddJsonBody(new { phone = payload.Phone, body = payload.Body, receiverName = payload.ReceiverName, microserviceName = payload.MicroserviceName });
                var apiresp = await MakeRequestAsync(null, url, method);
                Log.Information("SMS API Response - " + apiresp.Content);
                var responseData = JsonConvert.DeserializeObject<BaseSettlApiDTO>(apiresp.Content);
                return (responseData, apiresp.StatusCode);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
        }
    }
}