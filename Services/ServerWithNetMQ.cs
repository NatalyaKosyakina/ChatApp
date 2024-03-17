using ChatDB;
using LibLearning;
using NetMQ;
using NetMQ.Sockets;

namespace ChatApp.Services
{
    public class ServerWithNetMQ
    {
        Dictionary<String, NetMQFrame> clients = new Dictionary<string, NetMQFrame>();
        public IMessageSource<NetMQMessage> messageSource { get; set; } = new MessageSourceWithNetMQ();
        bool work = true;
        public ServerWithNetMQ() { }

        public void Work()
        {
            using (var socket = new RouterSocket())
            {
                socket.Bind("tcp://*:5556");
                Console.WriteLine("Сервер запущен");
                while (work)
                {
                    try
                    {
                        var msg = socket.ReceiveMultipartMessage();
                        var message = messageSource.Receive(ref msg);

                        if (message == null)
                            return;

                        ProcessMessage(message, msg);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Ошибка при обработке сообщения: " + ex.Message);
                    }
                }
            }
        }

        private void ProcessMessage(ChatMessage message, NetMQMessage fromep)
        {
            Console.WriteLine($"Получено сообщение от {message.FromName} для {message.ToName} с командой {message.Command}:");
            Console.WriteLine(message.Text);


            if (message.Command == LibLearning.Command.Register)
            {
                Register(message, messageSource.CopyT(fromep));

            }
            if (message.Command == LibLearning.Command.Confirmation)
            {
                Console.WriteLine("Confirmation receiver");
                ConfirmMessageReceived(message.Id);
            }
            if (message.Command == LibLearning.Command.Message)
            {
                RelyMessage(message);
            }
        }

        private void RelyMessage(ChatMessage message)
        {
            int? id = null;
            if (clients.TryGetValue(message.ToName, out NetMQFrame ep))
            {
                using (var ctx = new MyAppContext())
                {
                    var fromUser = ctx.Users.First(x => x.Login == message.FromName);
                    var toUser = ctx.Users.First(x => x.Login == message.ToName);
                    var msg = new Message { Autor = fromUser, Consumer = toUser, IsReceived = false, Text = message.Text };
                    ctx.Messages.Add(msg);

                    ctx.SaveChanges();

                    id = msg.ID;
                }
                var forwardMessage = new ChatMessage() { Id = id, Command = LibLearning.Command.Message, ToName = message.ToName, FromName = message.FromName, Text = message.Text };
                NetMQMessage nmsg = new NetMQMessage();
                nmsg.Append(ep);
                messageSource.Send(forwardMessage, nmsg);

                Console.WriteLine($"Message Relied, from = {message.FromName} to = {message.ToName}");
            }
            else
            {
                Console.WriteLine("Пользователь не найден.");
            }
        }

        private void ConfirmMessageReceived(int? id)
        {
            Console.WriteLine("Message confirmation id=" + id);

            using (var ctx = new MyAppContext())
            {
                var msg = ctx.Messages.FirstOrDefault(x => x.ID == id);

                if (msg != null)
                {
                    msg.IsReceived = true;
                    ctx.SaveChanges();
                }
            }
        }

        private void Register(ChatMessage message, NetMQMessage fromep)
        {
            Console.WriteLine("Message Register, name = " + message.FromName);
            clients.Add(message.FromName, fromep[0]);


            using (var ctx = new MyAppContext())
            {
                if (ctx.Users.FirstOrDefault(x => x.Login == message.FromName) != null) return;

                ctx.Add(new User { Login = message.FromName });

                ctx.SaveChanges();
            }
        }
    }
}
