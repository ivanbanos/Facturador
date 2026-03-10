using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ManejadorSurtidor.Messages
{
    public  interface IMessageProducer
    {
        Task SendMessage<T>(T message, string queue);
    }
}
