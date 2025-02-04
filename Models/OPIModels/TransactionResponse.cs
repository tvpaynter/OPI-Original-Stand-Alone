using Destructurama.Attributed;
using System;
using System.Xml.Serialization;

namespace UTG.Models.OPIModels
{
	[XmlRoot(ElementName = "TransactionResponse")]
	public class TransactionResponse
	{

		[XmlElement(ElementName = "SequenceNo")]
		public string SequenceNo { get; set; }

		[XmlElement(ElementName = "TransType")]
		public string TransType { get; set; }

		[XmlElement(ElementName = "TransAmount")]
		public decimal? TransAmount { get; set; }

		[XmlIgnore]
		public bool TransAmountSpecified { get; set; }

		[XmlElement(ElementName = "OtherAmount")]
		public int OtherAmount { get; set; }

		[XmlIgnore]
		public bool OtherAmountSpecified { get; set; }

		[LogMasked(ShowLast = 4, PreserveLength = true)]
		[XmlElement(ElementName = "PAN")]
		public string PAN { get; set; }

		[LogMasked(Text = "*", PreserveLength = true)]
		[XmlElement(ElementName = "ExpiryDate")]
		public int ExpiryDate { get; set; }
		
		[XmlIgnore]
		public bool ExpiryDateSpecified { get; set; }

		[LogMasked(ShowFirst = 4, PreserveLength = true)]
		[XmlElement(ElementName = "TransToken")]
		public string TransToken { get; set; }

		[XmlElement(ElementName = "EntryMode")]
		public string EntryMode { get; set; }

		[XmlIgnore]
		public bool EntryModeSpecified { get; set; }

		[XmlElement(ElementName = "Balance")]
		public int Balance { get; set; }

		[XmlIgnore]
		public bool BalanceSpecified { get; set; }

		[XmlElement(ElementName = "IssuerId")]
		public string IssuerId { get; set; }

		[XmlElement(ElementName = "RespCode")]

		//In document respcode is in Int but we have alfa numeric response code from trx Services therefore chnaging int to string
		public string RespCode { get; set; }

		[XmlElement(ElementName = "RespText")]
		public string RespText { get; set; }

		[XmlElement(ElementName = "AuthCode")]
		public string AuthCode { get; set; }

		[XmlElement(ElementName = "RRN")]
		public string RRN { get; set; }

		[XmlElement(ElementName = "OfflineFlag")]
		public string OfflineFlag { get; set; }

		[XmlElement(ElementName = "MerchantId")]
		public string MerchantId { get; set; }

		[XmlElement(ElementName = "TerminalId")]
		public string TerminalId { get; set; }

		[XmlElement(ElementName = "PrintData")]
		public string PrintData { get; set; }

		[XmlElement(ElementName = "DCCIndicator")]
		public string DCCIndicator { get; set; }

		[XmlElement(ElementName = "AltReceiptInfo")]
		public string AltReceiptInfo { get; set; }

		[XmlElement(ElementName = "CardholderName")]
		public string CardholderName { get; set; }

		[XmlElement(ElementName = "APP")]
		public string APP { get; set; }

		[XmlElement(ElementName = "AID")]
		public string AID { get; set; }

		[XmlElement(ElementName = "TVR")]
		public string TVR { get; set; }

		[XmlElement(ElementName = "TSI")]
		public string TSI { get; set; }

		[XmlElement(ElementName = "TC")]
		public string TC { get; set; }

		[XmlElement(ElementName = "OtherInfo")]
		public string OtherInfo { get; set; }

		[XmlElement(ElementName = "ProviderID")]
		public string ProviderID { get; set; }

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

		[XmlElement(ElementName = "TipAmount")]
		public string TipAmount { get; set; }
	}
}
