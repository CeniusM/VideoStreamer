using System.Net;
using System.Net.Sockets;
using System.Text;

namespace VideoStreaming;

class VideoPlayer
{
    static void Main()
    {
        int FrameHeight = 0;
        int FrameWidth = 0;
        int FrameCount = 0;




        string ip = "";
        while (true)
        {
            Console.Write("Type in ip: ");
            ip = Console.ReadLine()!;

            if (IPAddress.TryParse(ip, out IPAddress adress))
            {
                if (adress is null)
                    continue;

                TcpClient client = new TcpClient();
                client.Connect(IPAddress.Parse(ip), 3000);

                if (!client.Connected)
                    continue;

                NetworkStream stream = client.GetStream();

                byte[] bytes = new byte[12];
                stream.Read(bytes, 0, 12);

                FrameHeight = BitConverter.ToInt32(bytes, 0);
                FrameWidth = BitConverter.ToInt32(bytes, 4);
                FrameCount = BitConverter.ToInt32(bytes, 8);

                stream.Write(BitConverter.GetBytes(adress.Address));

                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 3000);
                UdpClient udp = new UdpClient(endPoint);

                List<byte> data = new List<byte>();
                byte[] line = new byte[FrameWidth * 2];

                Console.SetCursorPosition(0, 0);

                int frameLine = 0;

                bool running = true;
                // Play loop
                while (running)
                {
                    data.AddRange(udp.Receive(ref endPoint));

                    while (data.Count > FrameWidth + 1)
                    {
                        data.CopyTo(0, line, 0, FrameWidth + 1);
                        data.RemoveRange(0, FrameWidth + 1);
                        Console.WriteLine(ASCIIEncoding.UTF8.GetString(line, 0, FrameWidth));

                        frameLine++;
                        if (frameLine == FrameHeight - 1)
                        {
                            Console.SetCursorPosition(0, 0);
                            frameLine = 0;
                        }

                        //// end of Frame
                        //if (line[FrameWidth] == 1)
                        //{

                        //    Console.SetCursorPosition(0, 0);
                        //    Thread.Sleep(100);
                        //}
                        //// end of frames
                        //else if (line[FrameWidth] == 2)
                        //{
                        //    running = false;
                        //    break;
                        //}
                    }

                }

                // End
                client.Dispose();
                stream.Dispose();
                udp.Dispose();

                Console.WriteLine("Done, Press enter to continue");
                Console.ReadLine();
            }

            Console.Clear();
            Console.SetCursorPosition(0, 0);
        }
    }



    // Thanks Stackoverflow
    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }
}