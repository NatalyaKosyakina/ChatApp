using ChatApp.Services;
using LibLearning;
using NetMQ;
using System.Net;
using System.Text.Json;
namespace ChatApp
{
    internal class Program
    {
        public static void Run(string[] args)
        {
            if (args.Length == 0)
            {
                var server = new ServerWithNetMQ();
                server.Work();
            }
            else
            {
                var cl1 = new MessageSourceClientWithNetMQ();
                Console.WriteLine("Укажите логин");
                string info = Console.ReadLine();
                if (string.IsNullOrEmpty(info))
                {
                    info = "Неопознанный бобр";
                }
                var client = new ClientWithNetMQ(info, cl1);
                client.Start();
            }
        }
        static void Main(string[] args)
        {
            Run(args);
        }
    }
}
