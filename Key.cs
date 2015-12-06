using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchDashPC
{
    class Key
    {
        private String key = "";
        private String code = "";
        public static Dictionary<String, String> ModifierKeys = new Dictionary<String, String> { 
            {"BACKSPACE", "{BACKSPACE}"},
            {"BREAK", "{BREAK}"},
            {"CAPS LOCK", "{CAPSLOCK}"},
            {"DEL", "{DEL}"},
            {"DOWN ARROW", "{DOWN}"},
            {"END", "{END}"},
            {"ENTER", "{ENTER}"},
            {"ESC", "{ESC}"},
            {"HELP", "{HELP}"},
            {"HOME", "{HOME}"},
            {"INS", "{INS}"},
            {"LEFT ARROw", "{LEFT}"},
            {"NUM LOCK", "{NUMLOCK}"},
            {"PAGE DOWN", "{PGDN}"},
            {"PAGE UP", "{PGUP}"},
            {"PRINT SCREEN", "{PRTSC}"},
            {"RIGHT ARROW", "{RIGHT}"},
            {"SCROLL LOCK", "{SCROLLLOCK}"},
            {"TAB", "{TAB}"},
            {"UP ARROW", "{UP}"},
            {"F1", "{F1}"},
            {"F2", "{F2}"},
            {"F3", "{F3}"},
            {"F4", "{F4}"},
            {"F5", "{F5}"},
            {"F6", "{F6}"},
            {"F7", "{F7}"},
            {"F8", "{F8}"},
            {"F9", "{F9}"},
            {"F10", "{F10}"},
            {"F11", "{F11}"},
            {"F12", "{F12}"},
            {"F13", "{F13}"},
            {"F14", "{F14}"},
            {"F15", "{F15}"},
            {"F16", "{F16}"},
            {"+", "{ADD}"},
            {"-", "{SUBTRACT}"},
            {"*", "{MULTIPLY}"},
            {"/", "{DIVIDE}"}
        };

        public Key(String capname)
        {
            key = capname;
            if (capname.Length > 1)
            {
                try
                {
                    code = ModifierKeys[capname];
                }
                catch (Exception)
                {
                    throw new Exception("Key capname is invalid.");
                }
            }
            else
            {
                code = capname;
            }
        }

        public String getCode()
        {
            return code;
        }

        public String getKey()
        {
            return key;
        }

        public bool isSpecial()
        {
            return ModifierKeys.ContainsKey(key);
        }
    }
}
