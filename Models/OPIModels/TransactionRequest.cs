using Destructurama.Attributed;
using System;
using System.Xml.Serialization;

namespace UTG.Models.OPIModels
{

    [XmlRoot(ElementName = "TransactionRequest")]
    public class TransactionRequest
    {
        private string expiryDate;

        [XmlElement(ElementName = "SequenceNo")]
        public string SequenceNo { get; set; }

        [XmlElement(ElementName = "TransType")]
        public string TransType { get; set; }

        [XmlElement(ElementName = "TransAmount")]
        public decimal? TransAmount { get; set; }

        [XmlElement(ElementName = "TransCurrency")]
        public string TransCurrency { get; set; }

        [XmlElement(ElementName = "TaxAmount")]
        public string TaxAmount { get; set; }

        [XmlElement(ElementName = "TransDateTime")]
        public DateTime TransDateTime { get; set; }

        [XmlElement(ElementName = "PartialAuthFlag")]
        public int PartialAuthFlag { get; set; }

        [XmlElement(ElementName = "SAF")]
        public int SAF { get; set; }

        [XmlElement(ElementName = "CardPresent")]
        public string CardPresent { get; set; }

        [XmlElement(ElementName = "SiteId")]
        public string SiteId { get; set; }

        [XmlElement(ElementName = "WSNo")]
        public string WSNo { get; set; }

        [XmlElement(ElementName = "Operator")]
        public string Operator { get; set; }

        [XmlElement(ElementName = "GuestNo")]
        public string GuestNo { get; set; }

        [XmlElement(ElementName = "ChargeInfo")]
        public string ChargeInfo { get; set; }

        [XmlElement(ElementName = "IndustryCode")]
        public int IndustryCode { get; set; }

        [XmlElement(ElementName = "ProxyInfo")]
        public string ProxyInfo { get; set; }

        [XmlElement(ElementName = "POSInfo")]
        public string POSInfo { get; set; }

        [LogMasked(ShowFirst = 4, PreserveLength = true)]
        [XmlElement(ElementName = "TransToken")]
        public string TransToken { get; set; }

        [LogMasked(ShowLast = 4, PreserveLength = true)]
        [XmlElement(ElementName = "PAN")]
        public string Pan { get; set; }

        [XmlElement(ElementName = "ExpiryDate")]

        [LogMasked(Text = "*", PreserveLength = true)]
        public string ExpiryDate
        {
            get
            {
                return expiryDate;
            }
            set
            {
                //DateTime date = DateTime.ParseExact(value, "yyMM", CultureInfo.InvariantCulture);
                expiryDate = value;//String.Format("{0}{1}", date.Month, date.ToString("yy"));
            }
        }

        [XmlElement(ElementName = "IssuerId")]
        public string IssuerId { get; set; }

        [XmlElement(ElementName = "LodgingCode")]
        public string LodgingCode { get; set; }

        [XmlElement(ElementName = "RoomRate")]
        public string RoomRate { get; set; }

        [XmlElement(ElementName = "CheckInDate")]
        public string CheckInDate { get; set; }

        [XmlElement(ElementName = "CheckOutDate")]
        public string CheckOutDate { get; set; }

        [XmlElement(ElementName = "AuthCode")]
        public string AuthCode { get; set; }

        [XmlElement(ElementName = "NewIncrementalAuth")]
        public string NewIncrementalAuth { get; set; }

        [XmlElement(ElementName = "OriginalRRN")]
        public string OriginalRRN { get; set; }

        [XmlElement(ElementName = "OtherAmount")]
        public string OtherAmount { get; set; }

        [XmlElement(ElementName = "ServiceChargeTtl")]
        public string ServiceChargeTtl { get; set; }

        [XmlElement(ElementName = "AutoServiceChargeTtl")]
        public string AutoServiceChargeTtl { get; set; }

        [XmlElement(ElementName = "NonRevenueServiceChargeTtl")]
        public string NonRevenueServiceChargeTtl { get; set; }

        [XmlElement(ElementName = "DiscountTtl")]
        public string DiscountTtl { get; set; }

        [XmlElement(ElementName = "CheckEmployeeCheckName")]
        public string CheckEmployeeCheckName { get; set; }

        [XmlElement(ElementName = "Covers")]
        public string Covers { get; set; }

        [XmlElement(ElementName = "CheckName")]
        public string CheckName { get; set; }

        [XmlElement(ElementName = "OrderType")]
        public string OrderType { get; set; }

        [XmlElement(ElementName = "TableTextAndGroup")]
        public string TableTextAndGroup { get; set; }

        [XmlElement(ElementName = "OriginalType")]
        public string OriginalType { get; set; }

        [XmlElement(ElementName = "OriginalTime")]
        public string OriginalTime { get; set; }
    }

}
