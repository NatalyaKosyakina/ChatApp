using System.Text.Json;
using LibLearning;
using NetMQ;
using NetMQ.Sockets;

namespace ChatApp.Services
{
    public class MessageSourceWithNetMQ : IMessageSource<NetMQMessage>
    {
        public RouterSocket? server {  get; set; }

        public MessageSourceWithNetMQ()
        {
            
        }
        public MessageSourceWithNetMQ(RouterSocket server)
        {
            this.server = server;
        }
        public NetMQMessage CopyT(NetMQMessage t)
        {
            NetMQMessage netMQFrames = new NetMQMessage();
            foreach (var item in t)
            {
                netMQFrames.Append(item);
            }
            return netMQFrames;
        }

        public NetMQMessage CreateNewT()
        {
            return new NetMQMessage();
        }


        /*Самое спорное решение!*/
        public ChatMessage Receive(ref NetMQMessage fromAddr)
        {
            ChatMessage msg = ChatMessage.FromJson(fromAddr[1].ConvertToString());
            return msg;
        }

        public void Send(ChatMessage message, NetMQMessage toAddr)
        {
            toAddr.Append(JsonSerializer.Serialize(message));
            server.SendMultipartMessage(toAddr);
        }
    }
}

