using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputController
{
    class KeyboardDevice
    {
        static HashSet<char> _upper = new HashSet<char>();
        static Dictionary<char, byte> _controlKey = new Dictionary<char, byte>();
        static Dictionary<char, byte> _letterKey = new Dictionary<char, byte>();
        static Dictionary<char, byte> _modifierKey = new Dictionary<char, byte>();

        public const byte KEYBOARD_INPUT_CMD = 0x05;
        public const char VK_IME_KEY = (char)0x0f;
        public const char VK_CAPITAL = (char)0x14;

        [Flags]
        public enum ModifierKey : byte
        {
            None,
            Left_Ctrl_Key = 0x01,
            Left_Shift_Key = 0x02,
            Left_Alt_Key = 0x04,
            Left_Window_Key = 0x08,

            Right_Ctrl_Key = 0x10,
            Hanja_Toggle_Key = 0x10,

            Right_Shift_Key = 0x20,

            Right_Alt_Key = 0x40,
            Hangul_Toggle_Key = 0x40,

            Right_Window_Key = 0x80,
        }

        static KeyboardDevice()
        {
            for (int i = (int)'A'; i < (int)'Z'; i++)
            {
                _upper.Add((char)i);
            }

            _upper.Add('~');
            _upper.Add('!');
            _upper.Add('@');
            _upper.Add('#');
            _upper.Add('$');
            _upper.Add('%');
            _upper.Add('^');
            _upper.Add('&');
            _upper.Add('*');
            _upper.Add('(');
            _upper.Add(')');
            _upper.Add('_');
            _upper.Add('+');
            _upper.Add('{');
            _upper.Add('}');
            _upper.Add('|');
            _upper.Add(':');
            _upper.Add('"');
            _upper.Add('<');
            _upper.Add('>');
            _upper.Add('?');

            // lower alphabet
            // 4 ~ 1d

            // 1e ~ 26
            {
                for (int i = 0; i < 9; i++)
                {
                    _letterKey.Add((char)('1' + i), (byte)(0x1e + i));
                }
                _letterKey.Add('0', 0x27);

                _letterKey.Add('!', 0x1e);
                _letterKey.Add('@', 0x1f);
                _letterKey.Add('#', 0x20);
                _letterKey.Add('$', 0x21);
                _letterKey.Add('%', 0x22);
                _letterKey.Add('^', 0x23);
                _letterKey.Add('&', 0x24);
                _letterKey.Add('*', 0x25);
                _letterKey.Add('(', 0x26);
                _letterKey.Add(')', 0x27);
            }

            _controlKey.Add('\r', 0x28);
            _controlKey.Add((char)ConsoleKey.Escape, 0x29);

            _controlKey.Add('\b', 0x2a);
            _controlKey.Add('\t', 0x2b);

            _letterKey.Add(' ', 0x2c);

            _letterKey.Add('-', 0x2d);
            _letterKey.Add('_', 0x2d);

            _letterKey.Add('=', 0x2e);
            _letterKey.Add('+', 0x2e);

            _letterKey.Add('[', 0x2f);
            _letterKey.Add('{', 0x2f);
            _letterKey.Add(']', 0x30);
            _letterKey.Add('}', 0x30);

            _letterKey.Add('\\', 0x31);
            _letterKey.Add('|', 0x31);

            // 0x32 ???
            _letterKey.Add(';', 0x33);
            _letterKey.Add(':', 0x33);

            _letterKey.Add('\'', 0x34);
            _letterKey.Add('"', 0x34);

            _letterKey.Add('`', 0x35);
            _letterKey.Add('~', 0x35);

            _letterKey.Add(',', 0x36);
            _letterKey.Add('<', 0x36);

            _letterKey.Add('.', 0x37);
            _letterKey.Add('>', 0x37);

            _letterKey.Add('/', 0x38);
            _letterKey.Add('?', 0x38);


            // ConsoleKey.CapsLock == 0x14(20)
            _controlKey.Add(VK_CAPITAL, 0x39);

            // 3a ~ 45
            for (int i = 0; i < 12; i++)
            {
                _controlKey.Add((char)(ConsoleKey.F1 + i), (byte)(0x3a + i));
            }

            // 0x46 (Open OneDrive Dialog)
            // 0x47 ???
            // 0x48 ???

            _controlKey.Add((char)ConsoleKey.Insert, 0x49);
            _controlKey.Add((char)ConsoleKey.Home, 0x4a);
            _controlKey.Add((char)ConsoleKey.PageUp, 0x4b);
            _controlKey.Add((char)ConsoleKey.Delete, 0x4c);
            _controlKey.Add((char)ConsoleKey.End, 0x4d);
            _controlKey.Add((char)ConsoleKey.PageDown, 0x4e);
            _controlKey.Add((char)ConsoleKey.RightArrow, 0x4f);

            _controlKey.Add((char)ConsoleKey.LeftArrow, 0x50);
            _controlKey.Add((char)ConsoleKey.DownArrow, 0x51);
            _controlKey.Add((char)ConsoleKey.UpArrow, 0x52);

            // 0x53 NumLock
            // 0x54 '/'
            // _controlKey.Add((char)ConsoleKey.Divide, 0x54);
            // 0x55 '*'
            // _controlKey.Add((char)ConsoleKey.Multiply, 0x55);
            // 0x56 '-'
            // _controlKey.Add((char)ConsoleKey.Subtract, 0x56);
            // 0x57 '+'
            // _controlKey.Add((char)ConsoleKey.Add, 0x57);
            // 0x58 ENTER (kp-enter)
            // 0x59 '1' ~ 0x61 '9'
            // _controlKey.Add((char)ConsoleKey.NumPad1, 0x59);
            //                     ~ ConsoleKey.NumPad9, 0x61
            // 0x62 '0'
            // _controlKey.Add((char)ConsoleKey.NumPad0, 0x62);
            // 0x63 '.'
            // _controlKey.Add((char)ConsoleKey.Decimal, 0x63);

            // 0x64 '\\'
            // 0x65 Fn + Ins
            // 0x66 ~ 0xff ???

            // _letterKey.Add((char)ConsoleKey.Multiply, 0x55);

            // https://msdn.microsoft.com/ko-kr/library/windows/desktop/dd375731(v=vs.85).aspx
            // 0x0f(15) == IME Hangul mode
            _modifierKey.Add(VK_IME_KEY, (byte)ModifierKey.Hangul_Toggle_Key);

            // 0x19(25) == HanjaKey
            _modifierKey.Add((char)0x19, (byte)ModifierKey.Hanja_Toggle_Key);

            _modifierKey.Add((char)ConsoleKey.LeftWindows, (byte)ModifierKey.Left_Window_Key);
            _modifierKey.Add((char)ConsoleKey.RightWindows, (byte)ModifierKey.Left_Window_Key);
        }

        public KeyboardDevice()
        {
        }

        public byte[] ConvertToKeyboardCommand(ConsoleKeyInfo key)
        {
            return ConsoleKeyInfoToCommand(new ConsoleKeyInfo[] { key });
        }

        public byte[] ConsoleKeyInfoToCommand(ConsoleKeyInfo[] keys)
        {
            List<byte> buf = new List<byte>();

            // 4byte = length
            buf.Add(0);
            buf.Add(0);
            buf.Add(0);
            buf.Add(0);

            buf.Add(KEYBOARD_INPUT_CMD);

            for (int i = 0; i < keys.Length; i++)
            {
                ConsoleKeyInfo key = keys[i];

                char ch = key.KeyChar;

                if (key.KeyChar == 0)
                {
                    if (_controlKey.ContainsKey((char)key.Key) == true)
                    {
                        buf.Add(GetModifierKey(key.Modifiers, false));
                        buf.Add(_controlKey[(char)key.Key]); // key down
                    }
                    else if (_modifierKey.ContainsKey((char)key.Key) == true)
                    {
                        buf.Add(_modifierKey[(char)key.Key]);
                        buf.Add(0);
                    }
                }
                else
                {
                    byte keyCode = CharToCode(ch);
                    bool needShift = _upper.Contains(ch);

                    byte modifier = GetModifierKey(key.Modifiers, needShift);
                    buf.Add(modifier);
                    buf.Add(keyCode); // key down
                }

                buf.Add(0);
                buf.Add(0);       // key up
            }

            int packetLen = (buf.Count - 4);
            byte[] lenBuf = BitConverter.GetBytes(packetLen);
            byte[] data = buf.ToArray();

            Array.Copy(lenBuf, data, 4);

            return data;
        }

        byte GetModifierKey(ConsoleModifiers modifiers, bool needShift)
        {
            byte modifierByte = 0;

            if ((modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
            {
                modifierByte |= (byte)ModifierKey.Left_Ctrl_Key;
            }

            if (needShift == true || (modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift)
            {
                modifierByte |= (byte)ModifierKey.Left_Shift_Key;
            }

            if ((modifiers & ConsoleModifiers.Alt) == ConsoleModifiers.Alt)
            {
                modifierByte |= (byte)ModifierKey.Left_Alt_Key;
            }

            return modifierByte;
        }

        public byte[] ConvertToKeyboardCommand(string txt)
        {
            List<ConsoleKeyInfo> buf = new List<ConsoleKeyInfo>();

            bool shiftPressed = false;
            bool ctrlPressed = false;
            bool altPressed = false;

            for (int i = 0; i < txt.Length; i++)
            {
                string cmdKey = "";

                if (txt[i] == '<')
                {
                    if (i != txt.Length - 1)
                    {
                        if (txt[i + 1] != '<')
                        {
                            int backPos = i;

                            cmdKey = ReadCommand(txt, ref i);
                            if (string.IsNullOrEmpty(cmdKey) == false)
                            {
                                bool processed = false;
                                ConsoleKeyInfo? key = CommandToKeyInfo(out processed, cmdKey, ref shiftPressed, ref altPressed, ref ctrlPressed);
                                if (processed == true)
                                {
                                    if (key != null)
                                    {
                                        buf.Add(key.Value);
                                    }
                                    continue;
                                }
                            }

                            i = backPos;
                        }
                        else
                        {
                            i++;
                        }
                    }
                }

                ConsoleKeyInfo cki = new ConsoleKeyInfo(
                    txt[i], ConsoleKey.NoName, shiftPressed, altPressed, ctrlPressed);

                buf.Add(cki);
            }

            return ConsoleKeyInfoToCommand(buf.ToArray());
        }

        private ConsoleKeyInfo? CommandToKeyInfo(out bool processed, string cmdKey, ref bool shiftPressed, ref bool altPressed, ref bool ctrlPressed)
        {
            processed = true;

            switch (cmdKey)
            {
                case "window":
                    return new ConsoleKeyInfo((char)0, ConsoleKey.LeftWindows, shiftPressed, altPressed, ctrlPressed);
                case "ime":
                    return new ConsoleKeyInfo((char)0, (ConsoleKey)VK_IME_KEY, shiftPressed, altPressed, ctrlPressed);
                case "return":
                    return new ConsoleKeyInfo('\r', ConsoleKey.NoName, shiftPressed, altPressed, ctrlPressed);
                case "ctrl_down":
                    ctrlPressed = true;
                    return null;
                case "ctrl_up":
                    ctrlPressed = false;
                    return null;
                case "shift_down":
                    shiftPressed = true;
                    return null;
                case "shift_up":
                    shiftPressed = false;
                    return null;
                case "alt_down":
                    altPressed = true;
                    return null;
                case "alt_up":
                    altPressed = false;
                    return null;
                case "capslock":
                    return new ConsoleKeyInfo(VK_CAPITAL, ConsoleKey.NoName, shiftPressed, altPressed, ctrlPressed);
                case "esc":
                    return new ConsoleKeyInfo((char)0, ConsoleKey.Escape, shiftPressed, altPressed, ctrlPressed);
                case "backspace":
                    return new ConsoleKeyInfo((char)0, ConsoleKey.Backspace, shiftPressed, altPressed, ctrlPressed);
                case "tab":
                    return new ConsoleKeyInfo((char)0, ConsoleKey.Tab, shiftPressed, altPressed, ctrlPressed);
                case "insert":
                    return new ConsoleKeyInfo((char)0, ConsoleKey.Insert, shiftPressed, altPressed, ctrlPressed);
                case "home":
                    return new ConsoleKeyInfo((char)0, ConsoleKey.Home, shiftPressed, altPressed, ctrlPressed);
                case "pageup":
                    return new ConsoleKeyInfo((char)0, ConsoleKey.PageUp, shiftPressed, altPressed, ctrlPressed);
                case "pagedown":
                    return new ConsoleKeyInfo((char)0, ConsoleKey.PageDown, shiftPressed, altPressed, ctrlPressed);
                case "del":
                    return new ConsoleKeyInfo((char)0, ConsoleKey.Delete, shiftPressed, altPressed, ctrlPressed);
                case "end":
                    return new ConsoleKeyInfo((char)0, ConsoleKey.End, shiftPressed, altPressed, ctrlPressed);
                case "left":
                    return new ConsoleKeyInfo((char)0, ConsoleKey.LeftArrow, shiftPressed, altPressed, ctrlPressed);
                case "right":
                    return new ConsoleKeyInfo((char)0, ConsoleKey.RightArrow, shiftPressed, altPressed, ctrlPressed);
                case "up":
                    return new ConsoleKeyInfo((char)0, ConsoleKey.UpArrow, shiftPressed, altPressed, ctrlPressed);
                case "down":
                    return new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, shiftPressed, altPressed, ctrlPressed);
            }

            if (cmdKey[0] == 'f' && cmdKey.Length >= 2)
            {
                char ch = cmdKey[1];
                if (ch >= '0' && ch <= '9')
                {
                    string txt = new string(ch, 1);
                    if (cmdKey.Length >= 3)
                    {
                        txt += cmdKey[2];
                    }

                    if (int.TryParse(txt, out int _result) == true)
                    {
                        return new ConsoleKeyInfo((char)0, ConsoleKey.F1 + _result - 1, shiftPressed, altPressed, ctrlPressed);
                    }
                }
            }

            processed = false;
            return null;
        }

        private string ReadCommand(string txt, ref int index)
        {
            if (txt.IndexOf(">", index) == -1)
            {
                return null;
            }

            List<char> cmd = new List<char>();

            index++;

            while (txt.Length > index)
            {
                if (txt[index] == '>')
                {
                    break;
                }

                cmd.Add(txt[index]);
                index++;
            }

            return new string(cmd.ToArray());
        }

        byte CharToCode(char ch)
        {
            if (char.IsLetter(ch) == true)
            {
                short lower = (short)char.ToLower(ch);
                byte keyCode = (byte)((lower - 'a') + 4);

                return keyCode;
            }

            if (_letterKey.ContainsKey(ch) == true)
            {
                return _letterKey[ch];
            }

            if (_controlKey.ContainsKey(ch) == true)
            {
                return _controlKey[ch];
            }

            return 0;
        }

    }
}
