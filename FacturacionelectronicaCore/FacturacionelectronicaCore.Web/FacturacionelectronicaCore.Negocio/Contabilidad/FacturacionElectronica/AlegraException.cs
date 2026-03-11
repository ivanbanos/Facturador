using System;
using System.Runtime.Serialization;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    [Serializable]
    internal class AlegraException : Exception
    {
        public AlegraException()
        {
        }

        public AlegraException(string message) : base(message)
        {
        }

        public AlegraException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AlegraException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}