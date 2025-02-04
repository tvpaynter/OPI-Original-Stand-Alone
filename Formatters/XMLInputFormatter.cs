using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UTG.Models.OPIModels;

namespace UTG.Formatters
{
    public class XMLInputFormatter : TextInputFormatter
    {
        public XMLInputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/xml"));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanReadType(Type type)
        {
            return type == typeof(TransactionRequest) || type ==  typeof(TokenRequest);
        } 
      

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(
       InputFormatterContext context, Encoding effectiveEncoding)
        {
            var httpContext = context.HttpContext;
            var buffer = new byte[Convert.ToInt32(httpContext.Request.ContentLength)];
            await httpContext.Request.Body.ReadAsync(buffer, 0, buffer.Length);
            //get body string here...
            var requestContent = Encoding.UTF8.GetString(buffer);
            httpContext.Request.Body.Position = 0;  //rewinding the stream to 0
            var request = new object();
            if (requestContent.Contains("TransactionRequest"))
                request = Common.Utils.DeserializeToObject<TransactionRequest>(requestContent);
            else
                request = Common.Utils.DeserializeToObject<TokenRequest>(requestContent);
            return await InputFormatterResult.SuccessAsync(request);
        }
    }
}
    