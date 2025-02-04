using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UTG.Models.OPIModels;

namespace UTG.Formatters
{
    public class XMLOutputFormatter : TextOutputFormatter
    {
        public XMLOutputFormatter() 
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/xml"));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanWriteType(Type? type)
        {
           return  typeof(TransactionResponse).IsAssignableFrom(type)
           || typeof(IEnumerable<TransactionResponse>).IsAssignableFrom(type)
           || typeof(TokenResponse).IsAssignableFrom(type)
           || typeof(IEnumerable<TokenResponse>).IsAssignableFrom(type);
        }
       

        public override async Task WriteResponseBodyAsync(
        OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var httpContext = context.HttpContext;

            var buffer = new StringBuilder();

            if (context.Object is IEnumerable<TransactionResponse> transResponses)
            {
                foreach (var transResponse in transResponses)
                {
                    buffer.Append(Common.Utils.Serialize(transResponse));
                }
            }
            else if(context.Object is IEnumerable<TokenResponse> tokenReponses)
            {
                foreach (var tokenReponse in tokenReponses)
                {
                    buffer.Append(Common.Utils.Serialize(tokenReponse));
                }
            }
            else if(context.Object is TokenResponse response)
            {
                buffer.Append(Common.Utils.Serialize(response));
            }
            else
            {
                buffer.Append(Common.Utils.Serialize((TransactionResponse)context.Object));
            }

            await httpContext.Response.WriteAsync(buffer.ToString(), selectedEncoding);
        }
    }
}
