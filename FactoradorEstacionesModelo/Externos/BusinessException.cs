using System;

namespace FacturacionelectronicaCore.Negocio.Modelo
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
