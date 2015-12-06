using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchDashPC
{
    class Command
    {
        public string CMD = null;
        public int VAL = -1;

        public Command(string command)
        {
            this.CMD = command;
        }

        public Command(string command, string value)
        {
            this.CMD = command;
            this.VAL = Int32.Parse(value);
        }
    }
}
