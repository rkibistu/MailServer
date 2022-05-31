using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client {
    class CClient : IClient{

        public byte Login(string username, string password) {

            Message message = new Message();
            message.Construct(Protocol.Login, username, password);

            MyConnection.Instance.SendToServer(message);

            //primeste mesaj cu SUCCED/FAILED
            message = MyConnection.Instance.GetFromServer();
            return message.m_instruction;
        }
    }
}
