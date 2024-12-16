using Newtonsoft.Json;
using Serilog;
using settl.identityserver.Application.Contracts.DTO.PushNotification;
using settl.identityserver.Domain.Shared.Enums;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Services
{
    public static class PushNotificationService
    {
        public static async Task<bool> SendAsync(PushNotificationRequestDTO model)
        {
            try
            {
                model.MicroserviceName = Constants.IDENTITYSERVER_URL;

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("X-RequestId", Guid.NewGuid().ToString());
                client.DefaultRequestHeaders.Add("X-Settl-Api-Token", Constants.SETTL_API_TOKEN);

                var url = $"{Constants.PUSHNOTIFICATIONSERVICE_URL}/notification/send";
                Log.Information($"API Call - URL:{url} | Headers: {JsonConvert.SerializeObject(client.DefaultRequestHeaders)} | Data: {JsonConvert.SerializeObject(model)}");
                var pushNotificationResponse = await client.PostAsJsonAsync(url, model);

                if (!pushNotificationResponse.IsSuccessStatusCode && pushNotificationResponse.Content is null) return false;
                var pushNotificationResult = await pushNotificationResponse.Content.ReadAsStringAsync();
                Log.Information("Push notification response " + pushNotificationResult);
                var response = JsonConvert.DeserializeObject<PushNotificationResponseDTO>(pushNotificationResult);

                if (response.Code != "00") return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return false;
            }
        }
    }
}