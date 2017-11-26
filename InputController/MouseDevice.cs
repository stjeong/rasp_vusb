using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputController
{
    class MouseDevice
    {
        public const byte MOUSE_REL_INPUT_CMD = 0x06;
        public const byte MOUSE_ABS_INPUT_CMD = 0x07;

        internal byte[] ConvertToMouseCommand(string txt)
        {
            List<byte> buf = new List<byte>();

            // 4byte = length
            buf.Add(0);
            buf.Add(0);
            buf.Add(0);
            buf.Add(0);

            buf.Add(0);

            string [] tokens = txt.Split(' ');
            bool isRelativeMouse = false;

            for (int i = 0; i < tokens.Length; i++)
            {
                byte [] input = TokenToCommand(tokens, ref i, ref isRelativeMouse);
                if (input == null)
                {
                    continue;
                }

                buf.AddRange(input);
                buf.AddRange(new byte[input.Length]);
            }

            int packetLen = (buf.Count - 4);
            byte[] lenBuf = BitConverter.GetBytes(packetLen);
            byte[] data = buf.ToArray();

            Array.Copy(lenBuf, data, 4);
            data[4] = (isRelativeMouse == true) ? MOUSE_REL_INPUT_CMD : MOUSE_ABS_INPUT_CMD;

            return data;
        }

        private byte[] TokenToCommand(string [] tokens, ref int tokenIndex, ref bool isRelativeMouse)
        {
            string token = tokens[tokenIndex];
            if (string.IsNullOrEmpty(token) == true)
            {
                return null;
            }

            isRelativeMouse = (token[0] == '+' || token[0] == '-');

            if (token[0] == 'w')
            {
                if (short.TryParse(token.Substring(1), out short wheel) == true)
                {
                    return new byte[] { 0, 0, 0, 0, (byte)-wheel };
                }

                return null;
            }

            bool hasX = short.TryParse(token, out short xPos);
            if (hasX == true)
            {
                if (tokens.Length == tokenIndex + 1)
                {
                    return null;
                }

                token = tokens[tokenIndex + 1];
                bool hasY = short.TryParse(token, out short yPos);
                if (hasY == true)
                {
                    tokenIndex++;

                    if (isRelativeMouse == false)
                    {
                        isRelativeMouse = (token[0] == '+' || token[0] == '-');
                    }

                    if (isRelativeMouse == true)
                    {
                        byte[] buf = new byte[] { 0, (byte)xPos, (byte)yPos };
                        return buf;
                    }
                    else
                    {
                        byte[] buf = new byte[] {
                            (byte)(xPos & 0xff),
                            (byte)((xPos & 0xff00) >> 8),
                            (byte)(yPos & 0xff),
                            (byte)((yPos & 0xff00) >> 8),
                            0,
                        };

                        return buf;
                    }
                }
            }

            switch (token)
            {
                case "b1":
                    isRelativeMouse = true;
                    return new byte[] { 0x01, 0, 0 };

                case "b2":
                    isRelativeMouse = true;
                    return new byte[] { 0x02, 0, 0 };

                case "b3":
                    isRelativeMouse = true;
                    return new byte[] { 0x04, 0, 0 };
            }

            return null;
        }
    }
}
