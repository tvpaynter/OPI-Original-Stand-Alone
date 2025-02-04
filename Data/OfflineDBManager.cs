using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UTG.Common;
using UTG.Common.Constants;
using UTG.Common.Handlers;
using UTG.Models;
using UTG.Models.OPIModels;
using UTG.Models.TerminalModels;

namespace UTG.Data
{
    public class OfflineDBManager : IOfflineDBManager
    {
        private readonly Config _databaseConfig;
        private readonly ILogger<OfflineDBManager> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpPostHandler _httpPostHandler;
        private bool inProcess;
        public OfflineDBManager(Config databaseConfig, ILogger<OfflineDBManager> logger, IConfiguration configuration, HttpPostHandler httpPostHandler)
        {
            _databaseConfig = databaseConfig;
            _logger = logger;
            _configuration = configuration;
            _httpPostHandler = httpPostHandler;
        }
        public async Task<IEnumerable<OfflineTransactionModel>> GetAllOfflineTransactions()
        {
            string query = "SELECT Id,Request, DateTime, TransactionType, Status, SequenceNo, TransToken, ResponseCode, Response, IV, JsonResponseFields FROM OfflineTransaction";
            using var connection = new SqliteConnection(_databaseConfig.ConnectionString);
            return await connection.QueryAsync<OfflineTransactionModel>(query);
        }
        public async Task<OfflineTransactionModel> GetOfflineTransacionByTransToken(string transToken)
        {
            var parameter = new { TransToken = transToken };
            string query = "SELECT Id,Request, DateTime, TransactionType, Status, SequenceNo, TransToken, ResponseCode, Response, IV, JsonResponseFields FROM OfflineTransaction where TransToken = @TransToken";
            using var connection = new SqliteConnection(_databaseConfig.ConnectionString);
            return await connection.QueryFirstOrDefaultAsync<OfflineTransactionModel>(query, parameter);
        }
        public async Task<IEnumerable<OfflineTransactionModel>> GetOfflineTransactions(int status)
        {
            var parameter = new { Status = status };
            var query = "SELECT Id,Request, DateTime, TransactionType, Status, SequenceNo, TransToken, ResponseCode, Response, IV, JsonResponseFields FROM OfflineTransaction where Status = @Status and TransactionType != '" + OPITransactionType.GetToken + "' and TransactionType != '" + OPITransactionType.Sale + "'and TransactionType != '" + OPITransactionType.Refund + "' Order by CASE" +
                            " WHEN TransactionType = '" + OPITransactionType.PreAuth + "' Then 0" +
                            " WHEN TransactionType = '" + OPITransactionType.IncrementalAuth + "' Then 1" +
                            " WHEN TransactionType = '" + OPITransactionType.SaleCompletion + "' Then 2" +
                            " WHEN TransactionType = '" + OPITransactionType.Void + "' Then 3" +
                            " END";
            using var connection = new SqliteConnection(_databaseConfig.ConnectionString);
            return await connection.QueryAsync<OfflineTransactionModel>(query, parameter);
        }
        public async void SaveOfflineTransaction(OfflineTransactionModel offlineTransactionModel)
        {
            using var connection = new SqliteConnection(_databaseConfig.ConnectionString);
            var insertQuery = "INSERT INTO OfflineTransaction (Request, DateTime, TransactionType, Status, SequenceNo, TransToken, IV, JsonResponseFields)" +
                "VALUES (@Request, @DateTime, @TransactionType, @Status, @SequenceNo, @TransToken, @IV, @JsonResponseFields)";
            await connection.ExecuteAsync(insertQuery, offlineTransactionModel);
        }
        public async void UpdateOfflineTransactionAsync(TransactionResponse opiresponse, int id, byte[] IV)
        {
            JsonResponseFields jsonResponseFields = new();
            jsonResponseFields.IssuerId = opiresponse.IssuerId;
            jsonResponseFields.MaskedPan = opiresponse.PAN;
            jsonResponseFields.UpdatedTransToken = opiresponse.TransToken;
            jsonResponseFields.ResponseCode = opiresponse.RespCode;

            string jsonResponse = JsonConvert.SerializeObject(jsonResponseFields);
            using var connection = new SqliteConnection(_databaseConfig.ConnectionString);
            var sqlQuery = $"UPDATE OfflineTransaction SET Response = @response, Status = @status, ResponseCode = @responsecode, JsonResponseFields = @JsonResponseFields  WHERE Id = @id;";
            var parameters = new DynamicParameters();
            parameters.Add("@response",
                Utils.EncryptPayload(JsonConvert.SerializeObject(opiresponse),
                GetEncryptionKey(), IV)
                );
            parameters.Add("@status", (int)UTGConstants.OfflineTransactionStatus.ProcessedWithHost);
            parameters.Add("@responsecode", opiresponse.RespCode);
            parameters.Add("@JsonResponseFields", jsonResponse);
            parameters.Add("@id", id);
            await connection.ExecuteAsync(sqlQuery, parameters);
        }
        public string GetEncryptionKey()
        {
            string query = "SELECT EncryptionKey FROM Settings";
            using var connection = new SqliteConnection(_databaseConfig.ConnectionString);
            var result = connection.QuerySingleOrDefaultAsync<string>(query);
            return Encoding.UTF8.GetString(Convert.FromBase64String(result.Result));
        }
        public async Task ProcessOfflineRequest()
        {
            if (!inProcess)
            {
                var offlineTransactions = await GetOfflineTransactions((int)UTGConstants.OfflineTransactionStatus.Added);
                if (offlineTransactions?.Count() > 0)
                {
                    _logger.Log(LogLevel.Information, "Total SNF Transaction :" + offlineTransactions.Count());
                }
                foreach (var transaction in offlineTransactions)
                {
                    inProcess = true;
                    _logger.Log(LogLevel.Information, "Sending offline request to host Service ,Ticket Number : " + transaction.TransToken);
                    string response = await _httpPostHandler.SendOfflineMessage
                        (Utils.DecryptPayload(transaction.Request,
                        GetEncryptionKey(), Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(Convert.FromBase64String(transaction.IV))))
                        );
                    _logger.Log(LogLevel.Information, $"Response from Host for offline request ,Ticket Number : {transaction.TransToken} : {response}");
                    if (string.IsNullOrEmpty(response))
                    {
                        break;
                    }
                    else
                    {
                        var Opiresponse = JsonConvert.DeserializeObject<TransactionResponse>(response);
                        UpdateOfflineTransactionAsync(Opiresponse, transaction.Id, Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(Convert.FromBase64String(transaction.IV))));
                    }

                }
                inProcess = false;
            }
        }
        public async Task DeleteProcessedOfflineTransaction(TokenRequestCardInfo[] tokenRequestCardInfos)
        {
            string[] parameter = tokenRequestCardInfos.Select(p => p.TransToken).ToArray();
            var transToken = string.Join(',', Array.ConvertAll(parameter, z => "'" + z + "'"));
            string query = $"DELETE FROM OfflineTransaction where TransToken in ({transToken})";
            using var connection = new SqliteConnection(_databaseConfig.ConnectionString);
            await connection.QueryAsync(query).ConfigureAwait(false);
        }

        public void UpdateOfflineTerminalTransaction(m4 m4Response, OfflineTransactionModel offlineTransactionModel)
        {
            JsonResponseFields jsonResponseFields = new();
            jsonResponseFields.IssuerId = m4Response.k9?.A3.ToUpper().GetIssuerID();
            jsonResponseFields.MaskedPan = "XXXXXXXXXXX" + m4Response.k9?.X9;
            jsonResponseFields.UpdatedTransToken = m4Response.k9?.U8;
            jsonResponseFields.ResponseCode = m4Response.m7?.m5;

            string jsonResponse = JsonConvert.SerializeObject(jsonResponseFields);
            using var connection = new SqliteConnection(_databaseConfig.ConnectionString);
            var sqlQuery = $"UPDATE OfflineTransaction SET Status = @status, ResponseCode = @responsecode, JsonResponseFields = @JsonResponseFields  WHERE Id = @id;";
            var parameters = new DynamicParameters();
            parameters.Add("@status", (int)UTGConstants.OfflineTransactionStatus.ProcessedWithTerminal);
            parameters.Add("@JsonResponseFields", jsonResponse);
            parameters.Add("@responsecode", m4Response.m7?.m5);
            parameters.Add("@id", offlineTransactionModel?.Id);
            connection.ExecuteAsync(sqlQuery, parameters);
        }

        public async Task<OfflineTransactionModel> GetOfflineTransacionByPurchaseId(string purchaseId)
        {
            var parameter = new { PurchaseId = purchaseId };
            string query = "SELECT * FROM OfflineTransaction WHERE json_extract(JsonResponseFields, '$.PurchaseId') = @PurchaseId;";
            using var connection = new SqliteConnection(_databaseConfig.ConnectionString);
            return await connection.QueryFirstOrDefaultAsync<OfflineTransactionModel>(query, parameter);
        }
    }
}
