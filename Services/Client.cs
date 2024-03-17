using LibLearning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Services
{
    public class Client<T>
    {
        readonly string name;
        readonly IMessageSourceClient<T> client;
        public Client(string n, IMessageSourceClient<T> cl)
        {
            this.name = n;
            client = cl;
        }

        void ClientListener()
        {
            T remoteEndPoint = client.CreateNewT();
            while (true)
            {
                try
                {
                    var messageReceived = client.Receive(ref remoteEndPoint);
                    Console.WriteLine($"Получено сообщение от {messageReceived.FromName}:");
                    Console.WriteLine(messageReceived.Text);

                    Confirm(messageReceived, remoteEndPoint);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при получении сообщения: " + ex.Message);
                }

            }
        }

        void Confirm(ChatMessage m, T remoteEndPoint)
        {
            var message = new ChatMessage() { FromName = name, ToName = null, Text = null, Id = m.Id, Command = Command.Confirmation };
            client.Send(message, remoteEndPoint);
        }


        void Register(T remoteEndPoint)
        {
            var chatMessage = new ChatMessage() { FromName = name, ToName = null, Text = null, Command = Command.Register };

            client.Send(chatMessage, remoteEndPoint);
        }

        void ClientSender()
        {
            Register(client.GetServer());

            while (true)
            {
                try
                {
                    Console.WriteLine("UDP Клиент ожидает ввода сообщения");

                    Console.Write("Введите  имя получателя и сообщение и нажмите Enter: ");
                    var messages = Console.ReadLine().Split(' ');

                    var message = new ChatMessage() { Command = Command.Message, FromName = name, ToName = messages[0], Text = messages[1] };

                    client.Send(message, client.GetServer());
                    Console.WriteLine("Сообщение отправлено.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при обработке сообщения: " + ex.Message);
                }
            }
        }
        public void Start()
        {
            new Thread(() => ClientListener()).Start();

            ClientSender();

        }
    }
}
