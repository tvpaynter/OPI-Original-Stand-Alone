using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using UTG.Common.Constants;
using UTG.Exceptions;
using UTG.Models.OPIModels;
using UTG.Models.TerminalModels;

namespace UTG.Common
{
    public static class Utils
    {
        public static string GetTempTransToken(string sequenceNo,string offlineTrasactionType)
        {
            return offlineTrasactionType + String.Format("{0:D12}", Int64.Parse(sequenceNo));
        }
        public static T DeserializeToObject<T>(string xmlData) where T : class
        {
            XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(T));
            try
            {
                using (StreamReader sr = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(xmlData))))
                {
                    return (T)ser.Deserialize(sr);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception raised in DeserializeToObject method : {ex.Message}");
                throw;
            }

        }
        public static string Serialize<T>(T dataToSerialize)
        {
            var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var serializer = new XmlSerializer(dataToSerialize.GetType());
            var settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;

            using (var stream = new StringWriter())
            using (var writer = XmlWriter.Create(stream, settings))
            {
                serializer.Serialize(writer, dataToSerialize, emptyNamespaces);
                return stream.ToString();
            }
        }
        public static XDocument GetMaskedRequest(string xmlRequest)
        {
            XDocument doc = new();
            try
            {
                doc = XDocument.Parse(xmlRequest);
                foreach (XElement element in doc.Descendants().Where(
                    e => e.Name.ToString().Contains("AcctNum")))
                {
                    element.Value = element.Value.MaskedCardNumber();
                }
                foreach (XElement element in doc.Descendants().Where(
                    e => e.Name.ToString().Contains("CCVData")))
                {
                    element.Value = "***";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception raised in GetMaskedRequest Method : {ex.Message}");
            }
            return doc;
        }

        public static string MaskedCardNumber(this string cardNumber)
        {
            if (!string.IsNullOrEmpty(cardNumber) && cardNumber.Length >= 15)
            {
                var lastDigits = cardNumber.Substring(cardNumber.Length - 4, 4);

                var requiredMask = new String('X', cardNumber.Length - 4);

                var maskedString = string.Concat(requiredMask, lastDigits);
                var maskedCardNumber = Regex.Replace(maskedString, ".{4}", "$0 ");
                return maskedCardNumber;
            }
            return cardNumber;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="errorMessage"></param>
        /// <param name="opiRequest"></param>
        /// <returns></returns>
        public static TransactionResponse BuildErrorResponse(string errorCode, string errorMessage, TransactionRequest opiRequest)
        {
            TransactionResponse response = new();
            response.RespText = errorMessage;
            response.RespCode = errorCode;
            response.SequenceNo = opiRequest.SequenceNo;
            response.TransType = opiRequest.TransType;
            response.MerchantId = opiRequest.SiteId;
            response.TransAmount = 0;
            response.RRN = "NA";
            response.TerminalId = opiRequest.WSNo;
            return response;

        }

        public static TransactionResponse BuildOnlineTransactionResponse(TransactionRequest opiRequest, m4 m4Response = null)
        {
            TransactionResponse response = new();
            if (m4Response?.m7?.m5 == UTGConstants.OPIApprovedRespCode)
            {
                response.RespText = UTGConstants.OPIIsOnlineOnlineRespText;
                response.OfflineFlag = "N";
            }
            else
            {
                response.RespText = UTGConstants.OPIIsOnlineOfflineRespText;
                response.OfflineFlag = "Y";
            }

            response.RespCode = UTGConstants.OPIApprovedRespCode;
            response.SequenceNo = opiRequest.SequenceNo;
            response.TransType = opiRequest.TransType;
            response.MerchantId = opiRequest.SiteId;
            response.RRN = "000000000000";
            response.TerminalId = opiRequest.WSNo;
            
            return response;
        }
        public static string GetXMLData(object xmlMssage)
        {
            MemoryStream memoryStream = new();
            XmlSerializer xs = new(xmlMssage.GetType());
            XmlTextWriter xmlTextWriter = new(memoryStream, System.Text.Encoding.UTF8);
            xs.Serialize(xmlTextWriter, xmlMssage);
            memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
            UTF8Encoding encoding = new();
            string xmlString = encoding.GetString(memoryStream.ToArray());
            xmlString = xmlString.Substring(1, xmlString.Length - 1);
            return xmlString;
        }
        /// <summary>
        /// To encrypt te request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string EncryptPayload(string request, string key,byte[] IV)
        {
            SymmetricAlgorithm CryptAlgorithm = (SymmetricAlgorithm)CryptoConfig.CreateFromName(UTGConstants.CryptoName);
            CryptAlgorithm.Key = Encoding.ASCII.GetBytes(key);
            CryptAlgorithm.Padding = PaddingMode.Zeros;
            CryptAlgorithm.Mode = CipherMode.CBC;
            CryptAlgorithm.IV = IV;
            ICryptoTransform icCryptor = CryptAlgorithm.CreateEncryptor();
            MemoryStream msCrypt = new(Encoding.ASCII.GetBytes(request));
            CryptoStream csCrypt = new(msCrypt, icCryptor, CryptoStreamMode.Write);
            byte[] bTemp = icCryptor.TransformFinalBlock(Encoding.ASCII.GetBytes(request), 0, Encoding.ASCII.GetBytes(request).Length);
            string result = Convert.ToBase64String(bTemp);

            return result;
        }
        /// <summary>
        ///  To Decrypt te request
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string DecryptPayload(string payload, string key, byte[] IV)
        {
            byte[] payload1 = Convert.FromBase64String(payload);
            SymmetricAlgorithm CryptAlgorithm = (SymmetricAlgorithm)CryptoConfig.CreateFromName(UTGConstants.CryptoName);
            CryptAlgorithm.Key = Encoding.ASCII.GetBytes(key);
            CryptAlgorithm.Padding = PaddingMode.Zeros;
            CryptAlgorithm.Mode = CipherMode.CBC;
            CryptAlgorithm.IV = IV;
            ICryptoTransform icCryptor = CryptAlgorithm.CreateDecryptor();
            MemoryStream msCrypt = new MemoryStream(payload1);
            CryptoStream csCrypt = new CryptoStream(msCrypt, icCryptor, CryptoStreamMode.Read);
            byte[] bTemp = icCryptor.TransformFinalBlock(payload1, 0, payload1.Length);
            // store decoded/decrypted response data
            return Encoding.ASCII.GetString(bTemp).Trim('\0');
        }
        private static byte[] ConvertHex(string sHex)
        {
            byte[] yKey;

            try
            {
                // set byte length
                yKey = new byte[sHex.Length / 2];

                for (int i = 0, j = 0; j < yKey.Length; i += 2, j++)
                {
                    yKey[j] = Convert.ToByte(sHex.Substring(i, 2), 16);
                }

            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }

            return yKey;
        }
        /// <summary>
        /// Calculate LRC 
        /// </summary>
        /// <param name="toEncode"></param>
        /// <returns></returns>
        public static char CalculateLRC(string toEncode)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(toEncode);
            byte LRC = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                LRC ^= bytes[i];
            }
            return Convert.ToChar(LRC);
        }
        /// <summary>
        /// Is domain alive
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static void IsDomainAlive(string ip, int port)
        {
            using (TcpClient client = new TcpClient())
            {
                var result = client.BeginConnect(ip, port, null, null);
                var succes = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(UTGConstants.PingTimeout));
                if (!succes)
                {
                    throw new ConnectivityException(UTGConstants.NotReachable, (int)HttpStatusCode.RequestTimeout);
                }
                client.EndConnect(result);
            }
        }
        /// <summary>
        /// Hex To String
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string HexToString(string hexString)
        {
            var bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                string currentHex = hexString.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(currentHex, 16);
            }
            var result = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            return result;
        }
        public static bool IsOnline(string url)
        {
            bool isOnline = true;
            Uri uri = new(url);
            try
            {
               Dns.GetHostAddresses(uri.Host);
            }
            catch
            {
                isOnline = false;
            }
            return isOnline;
        }

        public static string GetRandomIV()
        {
            StringBuilder randomIvsb = new();
            Random random = new();
            randomIvsb.Append(random.Next(10, 99));
            randomIvsb.Append(DateTime.Now.ToString("dd"));
            randomIvsb.Append(DateTime.Now.ToString("MM"));
            randomIvsb.Append(DateTime.Now.Year);
            randomIvsb.Append(DateTime.Now.ToString("HH"));
            randomIvsb.Append(DateTime.Now.ToString("mm"));
            randomIvsb.Append(DateTime.Now.ToString("ss"));
            return randomIvsb.ToString();
        }

        public static bool IsProcessWithTerminal(string transToken, string pan, string transType)
        {
            return string.IsNullOrEmpty(transToken) && 
                    string.IsNullOrEmpty(pan) &&
                    new[] { OPITransactionType.PreAuth,
                            OPITransactionType.Sale,
                            OPITransactionType.Refund,
                            OPITransactionType.GetToken,
                            OPITransactionType.IsOnline}.Contains(transType);
        }

        public static string StringToHex(string str)
        {
            var sb = new StringBuilder();

            var bytes = Encoding.Unicode.GetBytes(str);
            foreach (var t in bytes)
            {
                if (t != 0)
                {
                    sb.Append(t.ToString("X2"));
                }
            }

            return sb.ToString(); // returns: "48656C6C6F20776F726C64" for "Hello world"
        }
    }
}
