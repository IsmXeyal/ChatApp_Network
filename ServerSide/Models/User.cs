using System.Net.Sockets;

namespace ServerSide.Models;

public class User
{
    public string? Username { get; set; }
    public TcpClient? TcpClient { get; set; }
}
