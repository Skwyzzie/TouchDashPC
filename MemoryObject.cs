using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TouchDashPC
{
    public class MemoryObject
    {
        private int Address;
        private MemoryReaderEngine Engine;
        public MemoryObject(int address, MemoryReaderEngine engine)
        {
            this.Address = address;
            this.Engine = engine;
        }

        public int getAddress()
        {
            return Address;
        }

        public void setAddress(int address)
        {
            Address = address;
        }

        public int readInt()
        {
            String value = this.Engine.readMemory(Address);
            int conv = Int32.Parse(value);
            return conv;
        }

        public String readString()
        {
            return this.Engine.readMemory(Address);
        }
    }

    public class MemoryReaderEngine
    {
        const int PROCESS_WM_READ = 0x0010;
        private String AppName = "";
        private Process proc;
        private IntPtr procHndl;
        private List<MemoryObject> memObjects;

        public MemoryReaderEngine(String appname)
        {
            AppName = appname;
            proc = Process.GetProcessesByName(AppName)[0];
            procHndl = OpenProcess(PROCESS_WM_READ, false, proc.Id);
        }

        public String readMemory(Int32 address)
        {
            int bytesRead = 0;
            byte[] buffer = new byte[1024];

            ReadProcessMemory((int)procHndl, address, buffer, buffer.Length, ref bytesRead);

            return Encoding.Unicode.GetString(buffer);
        }


        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess,
          int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

    }
}
