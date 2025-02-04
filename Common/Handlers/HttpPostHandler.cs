using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UTG.Common.Constants;
using UTG.Exceptions;
using UTG.Models.OPIModels;

namespace UTG.Common.Handlers
{
    /// <summary>
    /// Http Post Handler
    /// </summary>
    public class HttpPostHandler
    {
        private IConfiguration _configuration { get; set; }
        private readonly ILogger<HttpPostHandler> _logger;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        public HttpPostHandler(ILogger<HttpPostHandler> logger, IConfiguration config)
        {
            _configuration = config;
            _logger = logger;
        }
        /// <summary>
        /// Send Message
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<string> SendMessage(TransactionRequest transactionRequest)
        {
            var request = JsonConvert.SerializeObject(transactionRequest);
            string response = string.Empty;
            string Url = _configuration["HostSettings:URL"];
            Uri uri = new(Url);
            int port = uri.Port;
            string domain;
            try
            {
                domain = System.Net.Dns.GetHostAddresses(uri.Host)[0].ToString();
            }
            catch (SocketException e)
            {
                _logger.Log(LogLevel.Error, "SocketException caught!!! ; Message : " + e.Message);
                throw new ConnectivityException(UTGConstants.NotReachable, (int)HttpStatusCode.ServiceUnavailable);
            }
            Utils.IsDomainAlive(domain, port);
            if (transactionRequest.TransType == OPITransactionType.IsOnline)
            {
                response = JsonConvert.SerializeObject(Utils.BuildOnlineTransactionResponse(transactionRequest));
            }
            else
            {
                /* Instantiate the WebRequest object.*/
                using (var client = new HttpClient())
                {
                    StringContent content = new(request, Encoding.UTF8, "application/json");
                    _logger.Log(LogLevel.Debug, "Sending Request To Host Service : " + Url);
                    HttpResponseMessage httpResponse = await client.PostAsync(Url, content);
                    response = httpResponse.Content.ReadAsStringAsync().Result;
                    _logger.Log(LogLevel.Debug, $"Response from Host :  {response}");
                }
            }
            return response;
        }

        public async Task<string> SendOfflineMessage(string request)
        {
            /*The XML request data to be posted to Rapid Connect Transaction Service URL*/
            string response = string.Empty;
            string Url = _configuration["HostSettings:URL"];
            try
            {
                using (var client = new HttpClient())
                {
                    StringContent content = new(request, Encoding.UTF8, "application/json");
                    HttpResponseMessage httpResponse = await client.PostAsync(Url, content);
                    response = httpResponse.Content.ReadAsStringAsync().Result;
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error while procesing offline transaction : {ex.Message}");
            }
            return response;
        }
    }
}
