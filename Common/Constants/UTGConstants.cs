using System.Collections;

namespace UTG.Common.Constants
{
    /// <summary>
    /// UTG Constants
    /// </summary>
    public static class UTGConstants
    {
        public static readonly char STX = (char)2;
        public static readonly char ETX = (char)3;
        public static readonly string PreAuth = "PREAUTH";
        public static readonly string SALE = "SALE";
        public static readonly string CryptoName = "Rijndael";
        public static readonly string OPIErrorRespCode = "06";
        public static readonly string OPIErrorRespText = "ERROR";
        public static readonly string OPIApprovedRespCode = "00";
        public static readonly string OfflineRespCode = "OL";
        public static readonly string OPIApprovedRespText = "APPROVAL";
        public static readonly string OPIOfflineApprovedRespText = "OFFLINE APPROVED";
        public static readonly string OPIOfflineAuthCode = "UTG";
        public static readonly string NotReachable = "HOST NOT REACHABLE";
        public static readonly int PingTimeout = 1000;
        public static readonly string OPIPartialApprovedRespCode = "10";
        public static readonly string OPIPartialApprovedRespText = "PARTIAL APPROVAL";
        public static readonly string OPINotFoundRespCode = "25";
        public static readonly string OPINotFoundRespText = "RECORD NOT FOUND";
        public static readonly string OPIIsOnlineOfflineRespText = "PROCESSED OFFLINE";
        public static readonly string OPIIsOnlineOnlineRespText = "PROCESSED ONLINE";
        public static readonly string OPIIncorrectPinRespCode = "55";
        public static readonly string OPIPinExceedsRespCode = "75";

        public enum OfflineTransactionStatus
        {
            Added,
            ProcessedWithHost,
            ProcessedWithOPI,
            ProcessedWithTerminal
        }
    }
    /// <summary>
    /// OPI Transaction Type
    /// </summary>
    public static class OPITransactionType
    {
        /// <summary>
        /// Pre Auth Const
        /// </summary>
        public const string PreAuth = "05";
        /// <summary>
        /// Incrmental Auth const
        /// </summary>
        public const string IncrementalAuth = "06";
        /// <summary>
        /// Sale Completion 
        /// </summary>
        public const string SaleCompletion = "07";
        /// <summary>
        /// Authorization Release Transaction
        /// </summary>
        public const string AuthRelease = "16";
        /// <summary>
        /// Void Transaction
        /// </summary>
        public const string Void = "08";
        /// <summary>
        /// Update Token
        /// </summary>
        public const string UpdateToken = "42";
        /// <summary>
        /// Reversal
        /// </summary>
        public const string Reversal = "04";
        /// <summary>
        /// Sale
        /// </summary>
        public const string Sale = "01";
        /// <summary>
        /// Refund
        /// </summary>
        public const string Refund = "03";
        /// <summary>
        /// GetToken
        /// </summary>
        public const string GetToken = "23";
        /// <summary>
        /// isOnline
        /// </summary>
        public const string IsOnline = "41";
        /// <summary>
        /// TransactionInquiry
        /// </summary>
        public const string TransactionInquiry = "20";
        /// <summary>
        /// TransactionInquiry
        /// </summary>
        public const string GetTokenBulk = "25";

    }
    /// <summary>
    /// Issuer Id Constant
    /// </summary>
    public static class IssuerId
    {
        /// <summary>
        /// hashtable
        /// </summary>
        public static readonly Hashtable hashtable = new Hashtable
            {
                { "VISA","01" },
                { "MASTERCARD", "02" },
                { "AMEX", "03" },
                { "DISCOVER", "12" },
                { "UNIONPAY","06"},
                { "JCB","05"},
                { "DISCOVER CREDIT","12"},
                { "VISA CREDIT","01"},
                { "VISA DEBIT","01"},
                { "AMERICAN EXPRESS","03"}
            };
        /// <summary>
        /// Get Trx Transaction Type
        /// </summary>
        /// <param name="strOPITransactionType"></param>
        /// <returns></returns>
        public static string GetIssuerID(this string strOPITransactionType)
        {
            return hashtable.Contains(strOPITransactionType) ? hashtable[strOPITransactionType].ToString() : "11";
        }
    }

    /// <summary>
    /// Transaction Type mapping
    /// </summary>
    public static class TransTypeMapping
    {
        /// <summary>
        /// hashtable
        /// </summary>
        public static readonly Hashtable hashtable = new Hashtable
            {
                { "05","AUTHORIZATION" },
                { "06", "AUTHORIZATION" },
                { "07", "SALE COMPLETION" }
            };
        public static readonly Hashtable TerminalTranTypesMapping = new()
        {
            { "05", "PREAUTH" },
            { "01", "SALE" },
            { "03", "RETURN" },
            { "41", "PING" }
        };
        /// <summary>
        /// Get Trx Transaction Type
        /// </summary>
        /// <param name="strOPITransactionType"></param>
        /// <returns></returns>
        public static string GetTransactionType(this string strOPITransactionType)
        {
            return hashtable[strOPITransactionType].ToString();
        }
        public static string GetTransactionTypeForTerminal(this string strOPITransactionType)
        {
            return TerminalTranTypesMapping[strOPITransactionType].ToString();
        }
    }

    /// <summary>
    /// EntryMode Constant
    /// </summary>
    public static class EntryMode
    {
        /// <summary>
        /// hashtable
        /// </summary>
        public static readonly Hashtable entryModeHashtable = new Hashtable
            {
                { "07","16" },//Tap //No CVM
                { "05", "25" },//EMV Chip //No CVM
                { "42","23"}, //EMV Chip with pin 
                { "5E","22"}, //EMV Chip with Signature 
                { "MANUAL", "21" }, // Manual No CVM
                {"MAGSTRIPE", "19" } // Swipe No CVM
            };
        /// <summary>
        /// Get Trx Transaction Type
        /// </summary>
        /// <param name="strOPITransactionType"></param>
        /// <returns></returns>
        public static string GetEntryMode(this string strTerminalEntryMode)
        {
            return entryModeHashtable.Contains(strTerminalEntryMode) ? entryModeHashtable[strTerminalEntryMode].ToString() : "19";
        }
    }
}
