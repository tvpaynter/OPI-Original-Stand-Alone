using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UTG.Common.Constants;
using UTG.Exceptions;

namespace UTG.Common.Handlers
{
    /// <summary>
    /// TCP handler class
    /// </summary>
    public class TcpIpHandler
    {
        private readonly IConfiguration _configuration;

        private readonly ILogger<TcpIpHandler> _logger;
        /// <summary>
        /// TCP Handler constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        public TcpIpHandler(ILogger<TcpIpHandler> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        /// <summary>
        /// Send Message to Terminal
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public async Task<string> SendMessage(string requestMessage)
        {
            _logger.Log(LogLevel.Debug, "In TCPHandler");
            byte[] outBytes = Encoding.ASCII.GetBytes(requestMessage);
            
            string response = string.Empty;
            IPAddress ipAddress = IPAddress.Parse(_configuration["TerminalSettings:IpAddress"]);
            IPEndPoint endPoint = new(ipAddress, Convert.ToInt32(_configuration["TerminalSettings:Port"]));
            int timeout = int.Parse(_configuration["TerminalSettings:Timeout"]);
            using (Socket senderSock = new(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                senderSock.SendTimeout = timeout;
                senderSock.ReceiveTimeout = timeout;
                senderSock.Connect(endPoint);
                _logger.Log(LogLevel.Debug, "Sending request to terminal");
                senderSock.Send(outBytes);
                StringBuilder sb = new();
                while (true)
                {
                    byte[] data = new byte[1024 * 4];
                    int bytesRec = senderSock.Receive(data, SocketFlags.None);
                    sb.Append(Encoding.ASCII.GetString(data, 0, bytesRec));
                    if (sb.ToString().IndexOf(UTGConstants.ETX) > -1)
                    {
                        break;
                    }
                }
                response = sb.ToString();
                _logger.Log(LogLevel.Debug, $"Response from terminal : {response} ");
                char[] newDelimiter = new char[] { '|' };
                var splitresponse = response.Split(newDelimiter, 3);
                if (splitresponse[1] == "00" || splitresponse[1] == "OL")
                {
                    response = splitresponse[2].Split(UTGConstants.ETX).First();
                    response = Regex.Replace(response, @"\s+", "");
                }
                else
                {
                    throw new OpiException(UTGConstants.OPIErrorRespText, (int)HttpStatusCode.BadRequest);
                }
                senderSock.Shutdown(SocketShutdown.Both);
                senderSock.Close();
            }
            return response;
        }


    }
}
