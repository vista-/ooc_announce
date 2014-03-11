using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using Meebey.SmartIrc4net;
using System.Threading;

namespace sendkeys_ss13
{
    public class Program
    {
        public static IrcClient irc = new IrcClient();
        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(HandleRef hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr WindowHandle);
        [DllImport("user32.dll")]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);
        public const int SW_RESTORE = 9;
        public const int BM_CLICK = 0x00F5;

        public static bool host = false;

        public static void OpMode()
        {
            Console.WriteLine("'host' or 'client' mode? (DreamDaemon and DreamSeeker respectively");
            string temp = Console.ReadLine();
            if (temp.ToLower() == "host")
                host = true;
            else if (temp.ToLower() == "client")
                host = false;
            else
            {
                Console.WriteLine("bad input");
                OpMode();
            }

        }
        public static void Main(string[]args) 
        {
            OpMode();
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
        public static void OnChannelMessage(object sender, IrcEventArgs e) 
        {
            if(e.Data.Nick == "vistapowa_tp")
            {
                string[] msg = e.Data.Message.Split(' ');
                string new_msg = "";
                for (int i = 0; i < msg.Length; i++)
                {
                    
                    msg[i] = Regex.Replace(msg[i], @"[\x02\x1F\x0F\x16]|\x03(\d\d?(,\d\d?)?)?", String.Empty);
                    char[] forbidden = {'{', '}', '[', ']', '+', '^', '%', '~', '(', ')'};
                    bool itwasbad = false;
                    for (int j = 0; j < msg[i].Length; j++)
			        {
                        if (itwasbad)
                        {
                            j--;
                            itwasbad = false;
                        }
                        for (int k = 0; k < forbidden.Length; k++)
                        {
                            if(forbidden[k] == msg[i][j])
                            {
                                int place = msg[i].Substring(j).IndexOf(forbidden[k]);
                                msg[i] = msg[i].Insert(j + place, "{");
                                msg[i] = msg[i].Insert(j + place + 2, "}");
                                if ((j + 3) <= (msg[i].Length - 1))
                                {
                                    j = j + 3;
                                    itwasbad = true;
                                }
                                else
                                {
                                    j = msg[i].Length;
                                    break;
                                }
                            }
                        }
                    }
                    if (i != msg.Length - 2)
                    {
                        new_msg += msg[i] + " ";
                    }
                }
                Thread.Sleep(100);
                if(msg[2] == "opened")
                {
                    Console.WriteLine("{0}", new_msg);
                    Process[] clientProcess = System.Diagnostics.Process.GetProcessesByName("dreamseeker");
                    Process[] hostProcess = System.Diagnostics.Process.GetProcessesByName("dreamdaemon");
                    if (!host)
                    {
                        if (clientProcess.Length > 0)
                        {
                            IntPtr hWnd = IntPtr.Zero;
                            hWnd = clientProcess[0].MainWindowHandle;
                            ShowWindowAsync(new HandleRef(null, hWnd), SW_RESTORE);
                            SetForegroundWindow(clientProcess[0].MainWindowHandle);
                            SendKeys.SendWait("ooc " + new_msg + "{ENTER}");
                        }
                    }
                    else
                    {
                        if(hostProcess.Length > 0)
                        {
                            IntPtr hWnd = IntPtr.Zero;
                            hWnd = hostProcess[0].MainWindowHandle;
                            IntPtr hWndPlayers = FindWindowEx(hWnd, IntPtr.Zero, "", "Players");
                            IntPtr hWndButton = FindWindowEx(hWndPlayers, IntPtr.Zero, "Button", "Send &Announcement"); ;
                           // ShowWindowAsync(new HandleRef(null, hWnd), SW_RESTORE);
                           // SetForegroundWindow(hostProcess[0].MainWindowHandle);
                            SendMessage(hWndButton, BM_CLICK, 1, IntPtr.Zero);
                            SendKeys.SendWait("ooc " + new_msg + "{ENTER}");
                        }
                    }
                }
            }
        } 
    }
}
