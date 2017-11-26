using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InputController
{
    class Program
    {
        EventWaitHandle _ewh = new EventWaitHandle(false, EventResetMode.ManualReset);

        static void Main(string[] args)
        {
            Program pg = new Program();
            pg.TestService();
        }

        private void TestService()
        {
            UsbController usbController = new UsbController();

            usbController.Connected += UsbController_Connected;
            usbController.ActivateFindService();

            bool mouseMode = true;

            _ewh.WaitOne();
            _ewh.Reset();

            while (true)
            {
                if (mouseMode == false)
                {
                    KeyboardTest(usbController);
                    mouseMode = true;
                }
                else
                {
                    MouseTest(usbController);
                    mouseMode = false;
                }
            }
        }

        private void UsbController_Connected(object sender, IPEndPoint e)
        {
            _ewh.Set();
        }

        private static void MouseTest(UsbController usbController)
        {
            while (true)
            {
                Console.Write("Mouse> ");
                string text = Console.ReadLine();
                if (text == "--mode")
                {
                    return;
                }

                usbController.MoveText(text);
            }
        }

        private static void KeyboardTest(UsbController usbController)
        {
            //{
            //    string txt = "abc+*()<ime>xptmxm<ime>";
            //    usbController.SendText(txt + Environment.NewLine);
            //}

            //{
            //    string txt = "<shift_down>abc<shift_up>";
            //    usbController.SendText(txt + Environment.NewLine);
            //}

            //{
            //    string txt = "<ctrl_down><esc><ctrl_up>";
            //    usbController.SendText(txt);
            //}

            //{
            //    string txt = "<capslock>test is good<capslock><return>";
            //    usbController.SendText(txt);
            //}

            //{
            //    string txt = "<ctrl_down><shift_down><esc><shift_up><ctrl_up>";
            //    usbController.SendText(txt);
            //}

            while (true)
            {
                Console.Write("Keyboard> ");
                string txt = Console.ReadLine();

                if (txt == "--mode")
                {
                    return;
                }

                if (txt.StartsWith("connect") == true)
                {
                    string[] connectParams = txt.Split();
                    usbController.Connect(connectParams[1], short.Parse(connectParams[2]));
                    continue;
                }

                usbController.SendText(txt);
            }

            //while (true)
            //{
            //    ConsoleKeyInfo key = Console.ReadKey();

            //    usbController.SendChar(key);
            //}

            //while (true)
            //{
            //    Console.ReadKey();

            //    List<byte> buf = new List<byte>();

            //    buf.Add(5);
            //    buf.Add(0);
            //    buf.Add(0);
            //    buf.Add(0);

            //    buf.Add(0x05); // KEYBOARD_INPUT_CMD

            //    buf.Add(0);
            //    buf.Add(4); // raw key code: 4 means 'a'

            //    buf.Add(0);
            //    buf.Add(0);

            //    usbController.SendKeyRawBuffer(buf.ToArray());
            //}
        }
    }
}
