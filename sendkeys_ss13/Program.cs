using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Meebey.SmartIrc4net;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace sendkeys_ss13
{
    public class Program
    {
        public static string serverIP = "127.0.0.1";
        public static int serverPort = 7281;

        public static IrcClient irc = new IrcClient();
        public static void Main(string[]args) 
        {
            ReadConf(ref serverIP, ref serverPort);
            irc.OnChannelMessage += new IrcEventHandler(OnChannelMessage);
            irc.SupportNonRfc = true;
            irc.Connect("irc.rizon.net" , 6670);
            Console.WriteLine("{DBG} Connected to IRC");
            irc.Login("vista_fun_bot", "vistas_fun_bot");
            Console.WriteLine("{DBG} Logged in");
            irc.RfcJoin("#fukkentestchannel");
            Console.WriteLine("{DBG} Joining #coderbus");
            irc.Listen();
        }

        public static void ReadConf(ref string IP, ref int port)
        {
            if (File.Exists("config.ini"))
            {
                StreamReader reader = new StreamReader("config.ini");
                while (!reader.EndOfStream)
                {
                    string[] line = reader.ReadLine().Split(' ');
                    Match match1 = Regex.Match(line[3], @"(\d{1,3}.?){4}"); //rudimentary IP validation
                    if (match1.Success)
                    {
                        serverIP = line[3];
                    }
                    else { Console.WriteLine("IP cannot be validated."); return; }
                    line = reader.ReadLine().Split(' ');
                    Match match2 = Regex.Match(line[3], @"(\d{1,5}"); //rudimentary port validation
                    if(match1.Success && (Convert.ToInt32(line[3]) < 65535))
                    {
                        serverPort = Convert.ToInt32(line[3]);
                    }
                    else { Console.WriteLine("Port cannot be validated."); return; }
                }
            }
            else { Console.WriteLine("Config file doesn't exist, using defaults"); return; }
        }
        public static void OnChannelMessage(object sender, IrcEventArgs e) 
        {
            if(e.Data.Nick == "vistapowa_tp")
            {
                string[] msg = e.Data.Message.Split(' ');
                for (int i = 0; i < msg.Length; i++)
                {
                    msg[i] = Regex.Replace(msg[i], @"[\x02\x1F\x0F\x16]|\x03(\d\d?(,\d\d?)?)?", String.Empty); //Sanitizing color codes
                }
                byte[] PACKETS = CreatePacket(msg);
                TcpClient client = new TcpClient(serverIP, serverPort);
                NetworkStream stream = client.GetStream();
                stream.Write(PACKETS, 0, PACKETS.Length);
                stream.Close();
                client.Close();
            }
        }

        private static byte[] CreatePacket(string[] msg)
        {
            StringBuilder packet = new StringBuilder();
            packet.Append('\x00');
            packet.Append('\x87');
            int len = 0;
            for (int i = 0; i < msg.Length; i++)
            {
                len += msg[i].Length;
            }
            packet.Append((char)(len + 6));
            for (int i = 0; i < 6; i++)
            {
                packet.Append('\x00');
            }
            for (int i = 0; i < msg.Length; i++)
            {
                packet.Append(msg[i]);
            }
            packet.Append('\x00');
            return Encoding.ASCII.GetBytes(packet.ToString()); 
        } 
    }
}
