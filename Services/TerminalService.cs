using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UTG.Common;
using UTG.Common.Constants;
using UTG.Common.Handlers;
using UTG.Data;
using UTG.Interfaces;
using UTG.Models;
using UTG.Models.OPIModels;
using UTG.Models.TerminalModels;
using UTG.StoreAndForward;

namespace UTG.Services
{
    /// <summary>
    /// Transaction Terminal service
    /// </summary>
    public class TerminalService : IUTGService
    {
        private readonly ILogger<TerminalService> _logger;
        private readonly IOfflineDBManager _offlineDBManager;
        private readonly TcpIpHandler _tcpIpHandler;
        private readonly ILogger<StoreAndForwardMiddleware> _snflogger;
        private const string mmlResponseStartTag = "<mmlresponse>";
        private const string mmlResponseEndTag = "</mmlresponse>";
        /// <summary>
        /// Trx Terminal Service
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="tcpIpHandler"></param>
        public TerminalService(ILogger<TerminalService> logger, TcpIpHandler tcpIpHandler, IOfflineDBManager offlineDBManager, ILogger<StoreAndForwardMiddleware> snflogger)
        {
            _logger = logger;
            _tcpIpHandler = tcpIpHandler;
            _offlineDBManager = offlineDBManager;
            _snflogger = snflogger;
        }
        /// <summary>
        /// Process incoming request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<TransactionResponse> ProcessMessage(TransactionRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Request from OPI : {@opiRequest}", request);
            string trxRequest = BuildTerminalRequest(request);
            var response = mmlResponseStartTag + await _tcpIpHandler.SendMessage(trxRequest) + mmlResponseEndTag;
            //Multipleresponse
            //var response = "<mmlresponse><m4><k9><U8>B7ANFZD7HPPV9BM</U8><w7>03/24/2022</w7><x1>11:58:16</x1><j3>00324222310044</j3><A3>Discover</A3><X9>3029</X9></k9><m7><m5>00</m5><m6>Approved</m6><D3>TRX653</D3></m7><AO><BL>8A|0000</BL><BL>91|3442579B71377E2D</BL><BL>72|40E3EDD5BE871453E7CCF424</BL><BL>DF79|373033</BL><BL>DF78|343530303438323936</BL><BL>50|446973636F766572</BL><BL>9F12|446973636F76657220437265646974</BL><BL>9F40|6000F0A001</BL><BL>9F02|000000001100</BL><BL>9F03|000000000000</BL><BL>9F06|A0000001523010</BL><BL>82|7800</BL><BL>9F36|015F</BL><BL>9F34|1E0300</BL><BL>9F39|05</BL><BL>9F33|E028C8</BL><BL>9F1A|0840</BL><BL>9F35|21</BL><BL>95|0000008000</BL><BL>5F2A|0840</BL><BL>9A|220324</BL><BL>9B|E800</BL><BL>9F21|115757</BL><BL>9C|00</BL><BL>9F37|E590FC0B</BL><BL>5F2D|656E6573</BL><BL>5F34|06</BL><BL>84|A0000001523010</BL><BL>5F20|4C455050454B2F4A554449544820202020202020202020202020</BL><BL>9F26|CD1ACDAF9E42766C</BL><BL>9F27|80</BL><BL>9F10|0105A08000800000</BL></AO></m4><m4><k9><U8>B7ANG4ZAHYW7U7J</U8><w7>03/24/2022</w7><x1>11:58:25</x1><j3>00324222310043</j3><A3>MasterCard</A3><X9>4111</X9></k9><m7><m5>00</m5><m6>Approved</m6><D3>TRX672</D3></m7><AO><BL>8A|0000</BL><BL>91|9DBA9A5C5D020B69</BL><BL>72|03B458A65E54AF2724928FF6</BL><BL>DF79|373033</BL><BL>DF78|343530303438323936</BL><BL>50|4D415354455243415244</BL><BL>9F12|4D617374657263617264</BL><BL>9F40|E000B05001</BL><BL>9F02|000000001100</BL><BL>9F03|000000000000</BL><BL>9F06|A0000000041010</BL><BL>82|3900</BL><BL>9F36|0004</BL><BL>9F34|5E0300</BL><BL>9F39|05</BL><BL>9F33|E028C8</BL><BL>9F1A|0840</BL><BL>9F35|21</BL><BL>95|0000008000</BL><BL>5F2A|0840</BL><BL>9A|220324</BL><BL>9B|E800</BL><BL>9F21|115626</BL><BL>9C|00</BL><BL>9F37|900A6794</BL><BL>5F2D|656E</BL><BL>5F34|01</BL><BL>84|A0000000041010</BL><BL>5F20|554154205553412F546573742043617264203037202020202020</BL><BL>9F26|DF72746AC5FAB493</BL><BL>9F27|80</BL><BL>9F10|0110A04009220000000000000000000000FF</BL></AO></m4></mmlresponse>";//"<mmlresponse>" + await _tcpIpHandler.SendMessage(trxRequest) +"</mmlresponse>" ;
            //Offline
            //var response = "<mmlresponse><m4><k9><w7>03/24/2022</w7><x1>11:57:32</x1><j3>00324222310043</j3><X9>4111</X9></k9><m7><m5>OL</m5><m6>Offline Approved</m6></m7><AO><BL>DF79|373033</BL><BL>DF78|343530303438323936</BL><BL>50|4D415354455243415244</BL><BL>9F12|4D617374657263617264</BL><BL>9F40|E000B05001</BL><BL>9F02|000000001100</BL><BL>9F03|000000000000</BL><BL>9F06|A0000000041010</BL><BL>82|3900</BL><BL>9F36|0004</BL><BL>9F34|5E0300</BL><BL>9F39|05</BL><BL>9F33|E028C8</BL><BL>9F1A|0840</BL><BL>9F35|21</BL><BL>95|0000008000</BL><BL>5F2A|0840</BL><BL>9A|220324</BL><BL>9B|E800</BL><BL>9F21|115626</BL><BL>9C|00</BL><BL>9F37|900A6794</BL><BL>5F2D|656E</BL><BL>5F34|01</BL><BL>84|A0000000041010</BL><BL>5F20|554154205553412F546573742043617264203037202020202020</BL><BL>9F26|DF72746AC5FAB493</BL><BL>9F27|80</BL><BL>9F10|0110A04009220000000000000000000000FF</BL></AO><w8><x2>Credit</x2></w8></m4></mmlresponse>";//"<mmlresponse>" + await _tcpIpHandler.SendMessage(trxRequest) +"</mmlresponse>" ;
            var MMLresponse = Utils.DeserializeToObject<mmlresponse>(response);
            _logger.LogInformation("Response from Terminal : {@response}", response);
            TransactionResponse trxResponse;
            //Get Smart Tag Details If Exists 
            Dictionary<string, string> smartTagsDict = MMLresponse.m4[0].AO?.BL?.ToDictionary(item => item.Split('|')[0], item => item.Split('|')[1]);

            if (MMLresponse.m4[0].m7.m5.Equals(UTGConstants.OfflineRespCode))
            {
                trxResponse = BuildOfflineResponse(MMLresponse.m4[0], request, smartTagsDict);
            }
            else
            {
                trxResponse = BuildOPIResponse(MMLresponse.m4[0], request, smartTagsDict);
            }
            _logger.LogInformation("Response send to OPI : {@trxResponse}", trxResponse);

            //async call to update existing offline transaction
            UpdateOfflineTransaction(MMLresponse, request);
            return trxResponse;
        }

        private async Task UpdateOfflineTransaction(mmlresponse mmlresponse, TransactionRequest transactionRequest)
        {
            if (mmlresponse.m4[0].m7.m5.Equals(UTGConstants.OfflineRespCode))
            {
                //Save Transaction
                OfflineTransactionModel offlineTransactionModel = new()
                {
                    TransactionType = transactionRequest.TransType,
                    SequenceNo = transactionRequest.SequenceNo,
                    DateTime = DateTime.UtcNow.ToString(),
                    Status = (int)UTGConstants.OfflineTransactionStatus.Added,
                    TransToken = Utils.GetTempTransToken(transactionRequest.SequenceNo, "OFT"),
                    JsonResponseFields = "{\"PurchaseId\":\"" + mmlresponse.m4[0].k9.j3 + "\"}"
                };
                _offlineDBManager.SaveOfflineTransaction(offlineTransactionModel);
            }
            else
            {
                foreach (var item in mmlresponse.m4.Skip(1))
                {
                    //Get Offline Transaction from DB
                    var OfflineTransaction = _offlineDBManager.GetOfflineTransacionByPurchaseId(item.k9.j3);
                    //UPdate Offline Transaction 
                    _offlineDBManager.UpdateOfflineTerminalTransaction(item, OfflineTransaction?.Result);
                }
            }
        }

        public Task<TokenResponse> ProcessTokenMessage(TokenRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// parse response
        /// </summary>
        /// <param name="response"></param>
        /// <param name="opiRequest"></param>
        /// <returns></returns>
        private TransactionResponse BuildOPIResponse(m4 response, TransactionRequest opiRequest, Dictionary<string, string> smartTagsDict)
        {
            if (opiRequest.TransType == OPITransactionType.IsOnline)
            {
                return Utils.BuildOnlineTransactionResponse(opiRequest, response);
            }
            string printdatasb = BuildPrintData(response, opiRequest, smartTagsDict);

            /* Parse the response*/
            TransactionResponse objResponse = new()
            {
                ExpiryDate = Convert.ToInt32(DateTime.Now.AddYears(2).ToString("yy") + "12"),
                ExpiryDateSpecified = true,
                EntryMode = GetEntryMode(smartTagsDict, response.w8),
                EntryModeSpecified = true,
                PAN = "XXXXXXXXXXX" + response.k9.X9,
                SequenceNo = opiRequest.SequenceNo,
                AuthCode = response.m7.D3,
                RespCode = BuildResponseCode(response.m7.m5),
                RespText = response.m7.m6,
                TransToken = response.k9.U8,
                RRN = response.k9.U8[3..],
                TransType = opiRequest.TransType,
                OfflineFlag = "N",
                PrintData = opiRequest.TransType == OPITransactionType.GetToken ? null : printdatasb,
                MerchantId = opiRequest.TransType == OPITransactionType.GetToken ? null : opiRequest.SiteId,
                TerminalId = opiRequest.TransType == OPITransactionType.GetToken ? null : opiRequest.WSNo,
                IssuerId = response.k9.A3.ToUpper().GetIssuerID(),
                TransAmount = opiRequest.TransType == OPITransactionType.GetToken ? null : Convert.ToDecimal(string.Format("{0:0}", opiRequest.TransAmount))
            };
            return objResponse;
        }

        private static string BuildResponseCode(string TerminalResponseCode)
        {
            return TerminalResponseCode switch
            {
                "55" => UTGConstants.OPIIncorrectPinRespCode,//IncorrectPin
                "75" => UTGConstants.OPIPinExceedsRespCode,//PinTriesExceeds
                _ => TerminalResponseCode,
            };
        }
        private static string BuildPrintData(m4 response, TransactionRequest opiRequest, Dictionary<string, string> smartTagsDict)
        {            
            StringBuilder printdatasb1 = new();
            printdatasb1.Append("##Merchant ID: " + opiRequest.SiteId);
            printdatasb1.Append("#Terminal ID: " + opiRequest.WSNo);
            printdatasb1.Append("#Card No. : XXXXXXXXXXX" + response.k9.X9);
            printdatasb1.Append("(C)#Expiry Date: XX/XX#Card Type: " + response.k9.A3);
            printdatasb1.Append("#Trans Type : " + (opiRequest.TransType == OPITransactionType.GetToken ? string.Empty : opiRequest.TransType.GetTransactionTypeForTerminal()));
            printdatasb1.Append("#Trans Time : " + response.k9.w7 + " " + response.k9.x1);
            printdatasb1.Append("#Trace No.: " + opiRequest.SequenceNo);
            printdatasb1.Append("#RRN: " + response.m7.m5 == UTGConstants.OfflineRespCode ? Utils.GetTempTransToken(opiRequest.SequenceNo, "OFT") : response.k9.U8[3..]);
            printdatasb1.Append("#Auth Code: " + response.m7.D3);
            printdatasb1.Append("##App Label : " + (smartTagsDict != null && smartTagsDict.ContainsKey("50") ? Utils.HexToString(smartTagsDict["50"]) : String.Empty));
            printdatasb1.Append("#AID: " + (smartTagsDict != null && smartTagsDict.ContainsKey("84") ? smartTagsDict["84"] : String.Empty));
            printdatasb1.Append("#AC: " + (smartTagsDict != null && smartTagsDict.ContainsKey("9F26") ? smartTagsDict["9F26"] : String.Empty));
            printdatasb1.Append("## BASE AMOUNT : USD" + string.Format("{0:0.00}", opiRequest.TransAmount / 100));
            printdatasb1.Append("###TIP: _______________###TOTAL: __________________");

            string printdata1 = "<MTYPE>#<MNAME>#<MCITY>, <MSTATE>,MD##";

            StringBuilder printdatasb = new();
            printdatasb.Append(printdata1);
            printdatasb.Append(" Merchant Copy");
            printdatasb.Append(printdatasb1);
            printdatasb.Append("###Signature: ____________________###I agree to the terms of my#credit agreement.@ ");
            printdatasb.Append(printdata1);
            printdatasb.Append(" Customer Copy");
            printdatasb.Append(printdatasb1);
            printdatasb.Append("###Transaction Approved with Signature##I agree to the terms of my#credit agreement.");
            return printdatasb.ToString();
        }
        private static string BuildOfflinePrintData(m4 response, TransactionRequest opiRequest, Dictionary<string, string> smartTagsDict)
        {
            StringBuilder printdatasb1 = new();
            printdatasb1.Append("##Merchant ID: " + opiRequest.SiteId);
            printdatasb1.Append("#Terminal ID: " + opiRequest.WSNo);
            printdatasb1.Append("#Card No. : XXXXXXXXXXX" + response.k9.X9);
            printdatasb1.Append("(C)#Expiry Date: XX/XX#Card Type: ");
            printdatasb1.Append("#Trans Type : " + (opiRequest.TransType == OPITransactionType.GetToken ? string.Empty : opiRequest.TransType.GetTransactionTypeForTerminal()));
            printdatasb1.Append("#Trans Time : " + response.k9.w7 + " " + response.k9.x1);
            printdatasb1.Append("#Trace No.: " + opiRequest.SequenceNo);
            printdatasb1.Append("#RRN: ");
            printdatasb1.Append("#Auth Code: " + response.m7.D3);
            printdatasb1.Append("##App Label : " + (smartTagsDict != null && smartTagsDict.ContainsKey("50") ? Utils.HexToString(smartTagsDict["50"]) : String.Empty));
            printdatasb1.Append("#AID: " + (smartTagsDict != null && smartTagsDict.ContainsKey("84") ? smartTagsDict["84"] : String.Empty));
            printdatasb1.Append("#AC: " + (smartTagsDict != null && smartTagsDict.ContainsKey("9F26") ? smartTagsDict["9F26"] : String.Empty));
            printdatasb1.Append("## BASE AMOUNT : USD" + string.Format("{0:0.00}", opiRequest.TransAmount / 100));
            printdatasb1.Append("###TIP: _______________###TOTAL: __________________");

            string printdata1 = "<MTYPE>#<MNAME>#<MCITY>, <MSTATE>,MD##";

            StringBuilder printdatasb = new();
            printdatasb.Append(printdata1);
            printdatasb.Append(" Merchant Copy");
            printdatasb.Append(printdatasb1);
            printdatasb.Append("###Signature: ____________________###I agree to the terms of my#credit agreement.@ ");
            printdatasb.Append(printdata1);
            printdatasb.Append(" Customer Copy");
            printdatasb.Append(printdatasb1);
            printdatasb.Append("###Transaction Approved with Signature##I agree to the terms of my#credit agreement.");
            return printdatasb.ToString();
        }

        private static string GetEntryMode(Dictionary<string, string> smartTagsDict, m4w8 cardentrymode)
        {
            if(cardentrymode != null &&  !string.IsNullOrWhiteSpace(cardentrymode.BP))
            {
                return cardentrymode.BP.ToUpper().GetEntryMode();
            }
            string entryTag = smartTagsDict != null && smartTagsDict.ContainsKey("9F39") ? smartTagsDict["9F39"] : null;
            string entryMode = entryTag != null ? entryTag.GetEntryMode() : "09"; //If 9F39 tag is not present then setting default value as 09
            if (entryTag == "05")
            {
                entryMode = smartTagsDict.ContainsKey("9F34") ? smartTagsDict["9F34"][..2].GetEntryMode() : entryMode;
            }
            return entryMode;
        }

        private static TransactionResponse BuildOfflineResponse(m4 response, TransactionRequest opiRequest, Dictionary<string, string> smartTagsDict)
        {
           
            Random random = new();
            TransactionResponse objResponse = new()
            {
                ExpiryDate = Convert.ToInt32(DateTime.Now.AddYears(2).ToString("yy") + "12"),
                ExpiryDateSpecified = true,
                EntryMode = GetEntryMode(smartTagsDict, response.w8),
                PAN = "XXXXXXXXXXX" + response.k9.X9,
                SequenceNo = opiRequest.SequenceNo,
                AuthCode = UTGConstants.OPIOfflineAuthCode + random.Next(100, 999),
                RespCode = UTGConstants.OPIApprovedRespCode,
                RespText = response.m7.m6,
                TransToken = Utils.GetTempTransToken(opiRequest.SequenceNo, "OFT"),
                //RRN = response.k9.U8[3..],
                TransType = opiRequest.TransType,
                OfflineFlag = "Y",
                IssuerId = (response.k9.A3 != null) ? response.k9.A3.ToUpper().GetIssuerID() : GetIssuerIdForOffline(smartTagsDict),
                TransAmount = null
            };
            if (opiRequest.TransType.Equals(OPITransactionType.Sale) || opiRequest.TransType.Equals(OPITransactionType.Refund))
            {
                //auth code is not getting in case of offline 
                response.m7.D3 = objResponse.AuthCode;
                string printdatasb = BuildOfflinePrintData(response, opiRequest, smartTagsDict);
                objResponse.PrintData = printdatasb;
                objResponse.MerchantId = opiRequest.SiteId;
                objResponse.TerminalId = opiRequest.WSNo;
            }

            return objResponse;
        }

        private static string GetIssuerIdForOffline(Dictionary<string, string> smartTagsDict)
        {
            string IssuerId = "01";
            if (smartTagsDict != null)
            {
                IssuerId = smartTagsDict.ContainsKey("9F12") ? Utils.HexToString(smartTagsDict["9F12"]).ToUpper().GetIssuerID() : null;
                if (IssuerId is null)
                {
                    IssuerId = smartTagsDict.ContainsKey("50") ? Utils.HexToString(smartTagsDict["50"]).ToUpper().GetIssuerID() : "01";
                }
            }
            return IssuerId;
        }
        /// <summary>
        /// Build the request in the form of terminal
        /// </summary>
        /// <param name="opiRequest"></param>
        /// <returns></returns>
        private string BuildTerminalRequest(TransactionRequest opiRequest)
        {
            //If Transaction is Get token we have to internally do Pre-Auth of $1 .
            var TransactionType = opiRequest.TransType;
            var amount = opiRequest.TransAmount;
            if (TransactionType == OPITransactionType.GetToken)
            {
                TransactionType = OPITransactionType.PreAuth;
                amount = 100;
            }
            //prepare trx transaction request based upon received OPI message
            //[02]PREAUTH|ID:1525870400|10.00[03]\
            //[02]SALE|ID:1525136702|16.37[03][00]B
            //[02]PING|ID:1830635938[03]P
            StringBuilder strBuilder = new();
            string[] elements = { TransactionType.GetTransactionTypeForTerminal(), "ID:" + opiRequest.SequenceNo, string.Format("{0:0.00}", amount / 100) };
            string terminalRequest = strBuilder.Append(UTGConstants.STX).
                                     AppendJoin("|", elements).
                                     Append(UTGConstants.ETX).
                                     Append(Utils.CalculateLRC
                                            (new StringBuilder().
                                                                 AppendJoin("|", elements).Append(UTGConstants.ETX).
                                                                 ToString())).ToString();
            _logger.Log(LogLevel.Information, $"Terminal Request : {terminalRequest}");
            return terminalRequest;
        }
    }
}
