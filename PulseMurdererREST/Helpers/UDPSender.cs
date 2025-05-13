using System.Net.Sockets;
using System.Text;

namespace PulseMurdererREST.Helpers{
    public static class UDPSender{
        public static void Send(string? message, string host = "127.0.0.1",int port = 13000){
            using(UdpClient udpClient = new()){
                byte[] data = Encoding.UTF8.GetBytes(message);
                udpClient.Send(data, data.Length, host, port);
            }
        }
    }
}
