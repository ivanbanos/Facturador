using System;

namespace FacturadorAPI.Models.Externos
{
    public class BusinessException : Exception
    {
        public BusinessException(string exceptionMessage)
        {
            ExceptionMessage = exceptionMessage;
        }

        public string ExceptionMessage { get; set; }
    }
}
