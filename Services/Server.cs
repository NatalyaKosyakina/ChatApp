using LibLearning;
using ChatDB;

namespace ChatApp.Services
{
    public class Server<T>
    {
        Dictionary<String, T> clients = new Dictionary<string, T>();

        public IMessageSource<T> messageSource { get; set; }
        bool work = true;

        public Server()
        {
            
        }
        public Server(IMessageSource<T> source)
        {
            messageSource = source;
        }
        //string ConnectionString = "Host=localhost;Username=postgres;Password=example;Database=ChatApp";

        void Register(ChatMessage message, T fromep)
        {
            Console.WriteLine("Message Register, name = " + message.FromName);
            clients.Add(message.FromName, fromep);


            using (var ctx = new MyAppContext())
            {
                if (ctx.Users.FirstOrDefault(x => x.Login == message.FromName) != null) return;

                ctx.Add(new User { Login = message.FromName });

                ctx.SaveChanges();
            }
        }

        void ConfirmMessageReceived(int? id)
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

        void RelyMessage(ChatMessage message)
        {
            int? id = null;
            if (clients.TryGetValue(message.ToName, out T ep))
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

                messageSource.Send(forwardMessage, ep);

                Console.WriteLine($"Message Relied, from = {message.FromName} to = {message.ToName}");
            }
            else
            {
                Console.WriteLine("Пользователь не найден.");
            }
        }

        void ProcessMessage(ChatMessage message, T fromep)
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
  
        public void Stop()
        {
            work = false;
        }
        public void Work()
        {
            Console.WriteLine("UDP Клиент ожидает сообщений...");
            while (work)
            {
                try
                {
                    T remoteEndPoint = messageSource.CreateNewT();
                    var message = messageSource.Receive(ref remoteEndPoint);

                    if (message == null)
                        return;

                    ProcessMessage(message, remoteEndPoint);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при обработке сообщения: " + ex.Message);
                }
            }
        }
    }

}
