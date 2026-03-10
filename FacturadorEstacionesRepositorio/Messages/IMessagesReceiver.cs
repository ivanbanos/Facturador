using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControladorEstacion.Messages
{
    public interface IMessagesReceiver
    {
        void ReceiveMessages(string queue);
    }
}
