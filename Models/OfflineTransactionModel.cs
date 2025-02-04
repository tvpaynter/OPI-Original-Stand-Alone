using Destructurama.Attributed;

namespace UTG.Models
{
    public class OfflineTransactionModel
    {
        public int Id { get; set; }
        public string Request { get; set; }
        public string DateTime { get; set; }
        public string TransactionType { get; set; }
        public int Status { get; set; }
        public  string SequenceNo { get; set; }
        [LogMasked(ShowFirst = 4, PreserveLength = true)]
        public string TransToken { get; set; }
        public string Response { get; set; }
        public string IV { get; set; }
        public string ResponseCode { get; set; }
        public string JsonResponseFields { get; set; }
    }

    public class JsonResponseFields {
        public string ResponseCode { get; set; }

        [LogMasked(ShowFirst = 4, PreserveLength = true)]
        public string UpdatedTransToken { get; set; }
        public string MaskedPan { get; set; }
        public string IssuerId { get; set; }
    }
}
