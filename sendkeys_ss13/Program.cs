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
        public const int SW_RESTORE = 9;

        public static void Main(string[]args) 
        { 
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
//              Console.WriteLine(e.Data.Message);
                if(msg[2] == "opened")
                {
                    Console.WriteLine("{0}", new_msg);
                    Process[] objProcesses = System.Diagnostics.Process.GetProcessesByName("dreamseeker");
                    if (objProcesses.Length > 0)
                    {
                        IntPtr hWnd = IntPtr.Zero;
                        hWnd = objProcesses[0].MainWindowHandle;
                        ShowWindowAsync(new HandleRef(null, hWnd), SW_RESTORE);
                        SetForegroundWindow(objProcesses[0].MainWindowHandle);
                        SendKeys.SendWait("ooc " + new_msg + "{ENTER}");
                    }
                }
            }
        } 
    }
}
