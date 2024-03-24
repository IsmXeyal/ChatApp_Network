using ClientSide.Command;
using ClientSide.Models;
using ClientSide.Views;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace ClientSide.ViewModels;

public class MainViewModel
{
    public MainView? MainView { get; set; }
    public ICommand? JoinCommand { get; set; }
    public ICommand? SendCommand { get; set; }
    public ObservableCollection<Message> UserMessages { get; set; }

    private TcpClient? _client;
    private IPAddress? clientIP;
    private int clientPort;
    // Clientin Connect olacaqi IPEndPoint
    private IPEndPoint? serverEP;

    private NetworkStream? clientData;
    private BinaryReader? reader;
    private BinaryWriter? writer;
    private bool isCheck = true;

    public MainViewModel(MainView mainView)
    {
        MainView = mainView;
        UserMessages = new ObservableCollection<Message>();
        
        JoinCommand = new RelayCommand(
            async action =>
            {
                try 
                {
                    clientIP = IPAddress.Parse(MainView.tbIP.Text);
                    clientPort = Convert.ToInt32(MainView.tbPort.Text);
                    serverEP = new IPEndPoint(clientIP, clientPort);
                    _client = new TcpClient();
                    await _client.ConnectAsync(serverEP);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            },
            pre => true);

        SendCommand = new RelayCommand(
            action =>
            {
                try
                {
                    if (_client != null && _client.Connected)
                    {
                        clientData = _client.GetStream();
                        writer = new BinaryWriter(clientData);

                        if (isCheck)
                        {
                            reader = new BinaryReader(clientData);
                            Task.Run(() => ReadMessagesFromServer());
                            writer.Write(MainView.tbname1.Text);
                            isCheck = false;
                        }
                        else
                        {
                            Message msg = new()
                            {
                                Username = MainView.tbname1.Text,
                                Acceptorname = MainView.tbname2.Text,
                                UserMessage = new TextRange(MainView.tbMessage.Document.ContentStart, MainView.tbMessage.Document.ContentEnd).Text,
                                Created = DateTime.Now
                            };
                            string jsonMessage = JsonSerializer.Serialize(msg);
                            writer.Write(jsonMessage);                           
                        }
                    }
                    else
                        MessageBox.Show("Not connected to the server.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            },
            pre => true);
    }

    public void ReadMessagesFromServer()
    {
        try
        {
            while (true)
            {
                var message = reader!.ReadString();

                var msg = JsonSerializer.Deserialize<Message>(message);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    UserMessages.Add(msg!);
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error receiving message: " + ex.Message);
        }
    }
}