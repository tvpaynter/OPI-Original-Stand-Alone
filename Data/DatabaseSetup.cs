using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text;
using UTG.Common;

namespace UTG.Data
{
    public class DatabaseSetup : IDatabaseSetup
    {
        private readonly Config _databaseConfig;
        private readonly ILogger<DatabaseSetup> _logger;

        public DatabaseSetup(Config databaseConfig, ILogger<DatabaseSetup> logger)
        {
           _databaseConfig = databaseConfig;
            _logger = logger;
        }
        public void SetupDB()
        {
            using var connection = new SqliteConnection(_databaseConfig.ConnectionString);
            var table = connection.Query<string>("SELECT name FROM sqlite_master WHERE type='table' AND name = 'OfflineTransaction';");
            var tableName = table.FirstOrDefault();
            if (string.IsNullOrEmpty(tableName) || tableName != "OfflineTransaction")
            {
                StringBuilder querysb = new();
                querysb.Append("CREATE TABLE OfflineTransaction ( ");
                querysb.Append(" Id INTEGER NOT NULL, " );
                querysb.Append(" Request TEXT NOT NULL, " );
                querysb.Append(" DateTime TEXT NOT NULL, " );
                querysb.Append(" TransactionType TEXT, " );
                querysb.Append(" Status INTEGER NOT NULL, " );
                querysb.Append(" SequenceNo TEXT, " );
                querysb.Append(" TransToken TEXT NOT NULL, " );
                querysb.Append(" UpdatedTransToken TEXT, " );
                querysb.Append(" ResponseCode TEXT, " );
                querysb.Append(" Response TEXT, " );
                querysb.Append(" IV BLOB NOT NULL, " );
                querysb.Append(" PRIMARY KEY(Id AUTOINCREMENT));");
                connection.Execute(querysb.ToString());
            }
        }
    }
}
