using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchDashPC
{
    class Macro : Key
    {
        private List<Key> keys = new List<Key>();
        private int timeBetweenPresses = 500;
        private int keyIndex = 0;


        public Macro(String command) : base(command)
        {
            for (int i = 0; i < command.Length; i++)
            {
                try
                {
                    Key key = new Key(command[i].ToString());
                    keys.Add(key);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public void setTimeBetweenPresses(int time)
        {
            timeBetweenPresses = time;
        }

        public int getTimeBetweenPresses()
        {
            return timeBetweenPresses;
        }

        public Key getNextKey()
        {
            return keys[++keyIndex];
        }

        public bool hasNextKey()
        {
            try
            {
                Key temp = keys[keyIndex + 1];
                return temp != null;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
