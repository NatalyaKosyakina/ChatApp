using LibLearning;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Services
{
    public class ClientWithNetMQ
    {
        readonly string name;
        MessageSourceClientWithNetMQ client;
        public ClientWithNetMQ(string n, MessageSourceClientWithNetMQ cl)
        {
            this.name = n;
            client = cl;
        }

        public void Start()
        {
            new Thread(() => ClientListener()).Start();

            ClientSender();

        }
        void ClientListener()
        {
            using (var listener = new DealerSocket())
            {
                while (true)
                {
                    client.client = listener;
                    listener.Connect("tcp://127.0.0.1:5556");
                    var msg = listener.ReceiveMultipartMessage();
                    try
                    {
                        var messageReceived = client.Receive(ref msg);
                        Console.WriteLine($"Получено сообщение от {messageReceived.FromName}:");
                        Console.WriteLine(messageReceived.Text);

                        Confirm(messageReceived, msg);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Ошибка при получении сообщения: " + ex.Message);
                    }

                }
            }
        }

        void ClientSender()
        {
            Register(client.GetServer());

            while (true)
            {
                try
                {
                    Console.WriteLine("Клиент ожидает ввода сообщения");

                    Console.Write("Введите  имя получателя и сообщение и нажмите Enter: ");
                    var messages = Console.ReadLine().Split(' ');

                    var message = new ChatMessage() { Command = Command.Message, FromName = name, ToName = messages[0], Text = messages[1] };

                    client.Send(message, client.GetServer());
                    Console.WriteLine("Сообщение отправлено.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при обработке сообщения: " + ex.StackTrace);
                }
            }
        }

        void Confirm(ChatMessage m, NetMQMessage msg)
        {
            var message = new ChatMessage() { FromName = name, ToName = null, Text = null, Id = m.Id, Command = Command.Confirmation };
            client.Send(message, msg);
        }

        void Register(NetMQMessage msg)
        {
            var chatMessage = new ChatMessage() { FromName = name, ToName = null, Text = null, Command = Command.Register };

            client.Send(chatMessage, msg);
        }
    }
}
