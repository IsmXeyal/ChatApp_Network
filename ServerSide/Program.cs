using System.Net.Sockets;
using System.Net;
using ServerSide.Models;
using System.Text.Json;
using ServerSide.DataBase;

namespace ServerSide;

public class Program
{
    public static void Main()
    {
        var ip = IPAddress.Parse("127.0.0.1");
        var port = 27001;

        var listenerEP = new IPEndPoint(ip, port);
        TcpListener listener = new(listenerEP);

        listener.Start();
        Console.WriteLine($"{listener.Server.LocalEndPoint} Server is Listener ....");

        while (true)
        {
            var client = listener.AcceptTcpClient();
            Console.WriteLine($"{client.Client.RemoteEndPoint} is connected ....");

            _ = Task.Run(() =>
            {
                var clientData = client.GetStream();
                var clientReader = new BinaryReader(clientData);
                var clientWriter = new BinaryWriter(clientData);

                bool isCheck = true;
                string username = "";
                while (true)
                {
                    if (isCheck)
                    {
                        username = clientReader.ReadString();
                        Database.Users.Add(new User()
                        {
                            Username = username,
                            TcpClient = client
                        });

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{username} is connected ....");
                        Console.ForegroundColor = ConsoleColor.White;
                        isCheck = false;
                    }
                    else if (!isCheck)
                    {
                        var message = clientReader.ReadString();

                        Message? msg = JsonSerializer.Deserialize<Message>(message);
                        if (msg is not null)
                        {
                            var user = Database.Users.FirstOrDefault(x => x.Username!.ToLower() == msg.Acceptorname!.ToLower());
                            if (user is not null)
                            {
                                BinaryWriter? userWriter = new(user.TcpClient!.GetStream());

                                Message? sendMsg = new()
                                {
                                    Acceptorname = username,
                                    UserMessage = msg.UserMessage,
                                    Created = msg.Created
                                };

                                var storeMessages = JsonSerializer.Serialize<Message>(sendMsg);
                                userWriter.Write(storeMessages);
                            }
                        }
                    }
                }
            });
        }
    }
}