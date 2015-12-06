using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace TouchDashPC
{
    public class KeyEngine
    {
        private String AppName = "";
        private IntPtr AppHandle = IntPtr.Zero;
        public KeyEngine(String appname)
        {
            AppName = appname;
            isAppRunning();
        }

        private bool isAppRunning()
        {
            AppHandle = IntPtr.Zero;
            Process[] p = Process.GetProcessesByName(AppName);
            if (p.Length > 0)
            {
                for (int i = 0; i < p.Length; i++)
                {
                    AppHandle = p[i].MainWindowHandle;
                }
            }
            return AppHandle != IntPtr.Zero;
        }

        public bool isRunning()
        {
            return isAppRunning();
        }

        public void send(String command)
        {
            if (isAppRunning())
            {
                if (command.Length > 1 && !Key.ModifierKeys.ContainsKey(command))
                {
                    Macro macro = new Macro(command);
                    macro.setTimeBetweenPresses(300);
                    while (macro.hasNextKey())
                    {
                        Key currentKey = macro.getNextKey();
                        SetForegroundWindow(AppHandle);
                        SendKeys.Send(currentKey.getCode());
                        System.Threading.Thread.Sleep(macro.getTimeBetweenPresses() - 5);
                    }
                }
                else
                {
                    Key key = new Key(command);
                    SetForegroundWindow(AppHandle);
                    SendKeys.Send(key.getCode());
                }
            }
            else
            {
                throw new Exception("Engine is not running. Please check initialization.");
            }
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
