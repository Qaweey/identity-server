using AutoMapper;
using Newtonsoft.Json;
using Serilog;
using settl.identityserver.Application.Contracts.DTO.Banking;
using settl.identityserver.Application.Contracts.IServices;
using settl.identityserver.Domain.Shared.Enums;
using settl.identityserver.Domain.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IMapper _mapper;

        public TransactionService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<(List<TransactionDTO>, string message)> Get(string phone)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-RequestId", Guid.NewGuid().ToString());
            client.DefaultRequestHeaders.Add("X-Settl-Api-Token", Constants.SETTL_API_TOKEN);
            var url = $"{Constants.BANKING_URL}/Transactions/phone/{phone}?Offset=0&Limit=5";
            Log.Information($"API Call Url: {url} Method:GET Headers: {JsonHelper.SerializeObject(client.DefaultRequestHeaders.ToList())}");
            var response = await client.GetAsync(url);

            var result = await response.Content.ReadAsStringAsync();

            Log.Information($"Banking response content - {result} | Status code - {response.StatusCode}");

            if (!response.IsSuccessStatusCode) throw new Exception("Unable to fetch transactions");

            var transactionResponse = JsonConvert.DeserializeObject<TransactionResponseDTO>(result);

            var transactions = _mapper.Map<IEnumerable<TransactionDTO>>(transactionResponse?.Data?.Transactions).ToList();

            return (transactions, transactionResponse?.Message);
        }
    }
}