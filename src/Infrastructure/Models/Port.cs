namespace AutoGame.Infrastructure.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class Port
    {
        public string Protocol;

        public string LocalAddress;

        public string ForeignAddress;

        public string State;

        public int ProcessId;

        public static bool TryParse(string s, out Port port)
        {
            var tokens = new Stack<string>(Regex.Split(s, "\\s+").Reverse());

            if (string.IsNullOrEmpty(tokens.Peek()))
            {
                tokens.Pop();
            }

            if (tokens.Count < 1)
            {
                port = null;
                return false;
            }

            port = new Port();

            port.Protocol = tokens.Pop();

            if (port.Protocol == "TCP")
            {
                if (tokens.Count < 4)
                {
                    port = null;
                    return false;
                }
            }
            else if (port.Protocol == "UDP")
            {
                if (tokens.Count < 3)
                {
                    port = null;
                    return false;
                }
            }
            else
            {
                port = null;
                return false;
            }

            port.LocalAddress = tokens.Pop();
            port.ForeignAddress = tokens.Pop();

            if (port.Protocol != "UDP")
            {
                port.State = tokens.Pop();
            }

            if (!int.TryParse(tokens.Pop(), out port.ProcessId))
            {
                port = null;
                return false;
            }

            return true;
        }
    }
}
