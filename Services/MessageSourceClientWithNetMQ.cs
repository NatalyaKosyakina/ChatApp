using LibLearning;
using NetMQ.Sockets;
using NetMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatApp.Services
{
    public class MessageSourceClientWithNetMQ : IMessageSourceClient<NetMQMessage>
    {
        public DealerSocket? client { get; set; }
        public NetMQMessage CreateNewT() => new NetMQMessage();
        /*В данной реализации методы CreateNewT и GetServer делают одно и то же - возвращают пустое сообщение, чтобы интерфейс не ругался.*/
        public NetMQMessage GetServer()
        {
            return new NetMQMessage();
        }

        public ChatMessage Receive(ref NetMQMessage fromAddr)
        {
            ChatMessage msg = ChatMessage.FromJson(fromAddr[1].ConvertToString());
            return msg;
        }

        public void Send(ChatMessage message, NetMQMessage toAddr)
        {
            toAddr.Append(JsonSerializer.Serialize(message));
            client.SendMultipartMessage(toAddr);
        }
    }
}
