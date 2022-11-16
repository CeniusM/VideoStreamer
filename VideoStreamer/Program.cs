using System.Net;
using System.Net.Sockets;

namespace VideoStreaming;

/*
 * Protocol: Streamer listends with Tcp on port 3000
 * When they connect streamer sends back height and width and count (12 bytes)
 * Then the Player returns their ip as a long
 * Then they establish a UDP connection and start emidiatly
 * 
 * To send the data just read and write from the bytes with a char pointer
 * 
 * 
 * */


class VideoStreamer
{
    const string FilePath = "C:\\Users\\ceniu\\source\\repos\\VideoStreamer\\VideoStreamer\\Frames.txt";

    static void Main()
    {
        int FrameHeight(ref List<string> file)
        {
            int result = 1;
            for (int i = 0; i < file.Count; i++)
                if (file[i].Contains('\0'))
                    break;
                else
                    result++;
            return result;
        }

        List<string> file = File.ReadAllLines(FilePath).ToList();
        //file = QuickFixes.FixDoubleLine(file);
        //File.WriteAllLines(FilePath, file);

        List<string[]> frames = new List<string[]>();

        int frameHeight = FrameHeight(ref file);
        int frameWidth = file[0].Length;
        //int frameSize = 30;
        int frameCount = file.Count / frameHeight;



        for (int FrameNum = 0; FrameNum < frameCount; FrameNum++)
        {
            string[] frame = new string[frameHeight];

            int fileStart = FrameNum * frameHeight;
            int fileEnd = FrameNum * frameHeight + frameHeight;

            // from start to end lines of frame i file
            for (int LineNum = fileStart; LineNum < fileEnd; LineNum++)
                frame[LineNum - fileStart] = file[LineNum];

            frames.Add(frame);
        }

        Console.WriteLine("Connect to: " + GetLocalIPAddress());

        // Make Tcp connection
        TcpListener listener = new TcpListener(IPAddress.Any, 3000);
        listener.Start();
        TcpClient client = listener.AcceptTcpClient();
        NetworkStream stream = client.GetStream();

        // Send frames stats
        List<byte> byteArray = new List<byte>();
        byteArray.AddRange(BitConverter.GetBytes(frameHeight));
        byteArray.AddRange(BitConverter.GetBytes(frameWidth));
        byteArray.AddRange(BitConverter.GetBytes(frameCount));
        stream.Write(byteArray.ToArray());

        // Get ip for UDP connection
        byte[] ipBytes = new byte[8];
        stream.Read(ipBytes, 0, 8);
        long ip = BitConverter.ToInt64(ipBytes);

        // Connect with Udp
        UdpClient streamer = new UdpClient();
        streamer.Connect(new IPEndPoint(ip, 3000));

        // Send data
        unsafe
        {
            try
            {
                byte[] line = new byte[frameWidth + 1];
                for (int i = 0; i < frames.Count; i++)
                {
                    //Console.SetCursorPosition(0,0);
                    Thread.Sleep(10);
                    string[] frame = frames[i];
                    for (int j = 0; j < frames[i].Length; j++)
                    {
                        System.Text.ASCIIEncoding.UTF8.GetBytes(frame[j], 0, frameWidth, line, 0);
                        if (i == frames.Count - 1)
                            line[frameWidth] = 2;
                        else if (j == frame.Length - 1)
                            line[frameWidth] = 1;
                        else
                            line[frameWidth] = 0;

                        streamer.Send(line);

                        //Console.WriteLine(System.Text.ASCIIEncoding.UTF8.GetString(line));



                        //fixed (char* charPtr = line)
                        //{
                        //    fixed (byte* bytePtr = (byte*)&charPtr[0])
                        //    {
                        //        streamer.Send(bytePtr, line.Lenght);
                        //    }
                        //}
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }
        }

        stream.Close();
        stream.Dispose();
        listener.Stop();
        streamer.Close();
        streamer.Dispose();

        Main();
    }

    //// For testing 
    //PlayFrames(frameSize, frames, 1);
    //public static void PlayFrames(int frameSize, List<string[]> frames, int FPS)
    //{
    //    void PrintFrame(string[] frame)
    //    {
    //        for (int i = 0; i < frame.Length; i++)
    //            Console.WriteLine(frame[i]);
    //    }

    //    for (int i = 0; i < frames.Count(); i++)
    //    {
    //        Console.CursorVisible = false;
    //        Console.SetCursorPosition(0,0);
    //        PrintFrame(frames[i]);
    //        Thread.Sleep(10);
    //    }

    //}


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