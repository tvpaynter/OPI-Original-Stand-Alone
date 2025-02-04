using FluentValidation;
using System.Collections.Generic;
using UTG.Common.Constants;
using UTG.Models.OPIModels;

namespace UTG.Validators
{
    public sealed class RequestValidator : AbstractValidator<TransactionRequest>
    {
        public RequestValidator(string transType)
        {
            RuleFor(transRequest => transRequest.TransType).NotNull();
            RuleFor(transRequest => transRequest.TransType).Must(CheckTransType)
                .WithMessage("Transaction Not Supported");
            RuleFor(transRequest => transRequest.SequenceNo).NotNull();
            if (transType != OPITransactionType.GetToken && transType != OPITransactionType.IsOnline)
            {
                RuleFor(transRequest => transRequest.TransAmount).NotNull();
                RuleFor(transRequest => transRequest.TransCurrency).NotNull();
            }
           
            RuleFor(transRequest => transRequest.TransDateTime).NotNull();
            RuleFor(transRequest => transRequest.SiteId).NotNull();
            RuleFor(transRequest => transRequest.WSNo).NotNull();
            if (transType != OPITransactionType.Reversal && transType != OPITransactionType.GetToken && transType != OPITransactionType.IsOnline && transType != OPITransactionType.TransactionInquiry)
            {
                RuleFor(transRequest => transRequest.Operator).NotNull();
            }
            RuleFor(transRequest => transRequest.IndustryCode).NotNull();
            RuleFor(transRequest => transRequest.ProxyInfo).NotNull();
            RuleFor(transRequest => transRequest.POSInfo).NotNull();

            switch (transType)
            {
                case OPITransactionType.IncrementalAuth:
                case OPITransactionType.Void:
                    RuleFor(transRequest => transRequest.OriginalRRN).NotNull();
                    RuleFor(transRequest => transRequest.TransToken).NotNull();
                    RuleFor(transRequest => transRequest.ExpiryDate).NotNull();
                    RuleFor(transRequest => transRequest.IssuerId).NotNull();
                    RuleFor(transRequest => transRequest.Pan).NotNull();
                    RuleFor(transRequest => transRequest.GuestNo).NotNull();
                    break;
                case OPITransactionType.SaleCompletion:
                    RuleFor(transRequest => transRequest.TransToken).NotNull();
                    RuleFor(transRequest => transRequest.IssuerId).NotNull();
                    RuleFor(transRequest => transRequest.Pan).NotNull();
                    RuleFor(transRequest => transRequest.ExpiryDate).NotNull();
                    RuleFor(transRequest => transRequest.AuthCode).NotNull();
                    RuleFor(transRequest => transRequest.OriginalRRN).NotNull();
                    RuleFor(transRequest => transRequest.GuestNo).NotNull();
                    break;
                case OPITransactionType.Reversal:
                case OPITransactionType.TransactionInquiry:
                    RuleFor(transRequest => transRequest.OriginalType).NotNull();
                    RuleFor(transRequest => transRequest.OriginalTime).NotNull();
                    break;
            }
        }
        
        private static bool CheckTransType(string transType)
        {
            List<string> ListOfSuportedTransType = new() { "01", "03", "04", "05", "06", "07", "08", "16", "42","23", "41", "20" };
            return ListOfSuportedTransType.Contains(transType); 
        }
    }
}
