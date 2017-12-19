using System;
using System.Net;
using System.Threading;

namespace InputController
{
    class Program
    {
        EventWaitHandle _ewh = new EventWaitHandle(false, EventResetMode.ManualReset);
        EventWaitHandle _exitEvent;
        UsbController _usbController;
        bool _exit;

        static void Main(string[] args)
        {
            Program pg = new Program();
            if (pg.WaitForExit(args) == true)
            {
                return;
            }

            pg.TestService();
        }

        public Program()
        {
            _usbController = new UsbController("testserver.com");
        }

        private bool WaitForExit(string[] args)
        {
            if (args.Length >= 1 && args[0] == "/exit")
            {
                new EventWaitHandle(false, EventResetMode.ManualReset, typeof(Program).Name).Set();

                int delay = 15;
                while (delay -- > 0)
                {
                    Console.Write(".");
                    Thread.Sleep(1000);
                }

                Console.WriteLine();
                return true;
            }
            else
            {
                _exitEvent = new EventWaitHandle(false, EventResetMode.ManualReset, typeof(Program).Name);

                Thread waitThread = new Thread(ExitProcess);
                waitThread.IsBackground = true;
                waitThread.Start();
                return false;
            }

            void ExitProcess(object eventHandle)
            {
                if (_exitEvent.WaitOne() == true)
                {
                    _exit = true;
                    _usbController.Shutdown();

                    Console.WriteLine("Shutdown signaled");
                }
            }
        }

        private void TestService()
        {
            _usbController.Connected += UsbController_Connected;
            _usbController.ActivateFindService();

            bool mouseMode = true;

            int cmdIndex = EventWaitHandle.WaitAny(new WaitHandle[] { _exitEvent, _ewh });
            if (cmdIndex == 0)
            {
                return;
            }

            while (_exit == false)
            {
                if (mouseMode == false)
                {
                    KeyboardTest();
                    mouseMode = true;
                }
                else
                {
                    MouseTest();
                    mouseMode = false;
                }
            }
        }

        private void UsbController_Connected(object sender, IPEndPoint e)
        {
            _ewh.Set();
        }

        private void MouseTest()
        {
            while (true)
            {
                Console.Write("Mouse> ");
                string text = Console.ReadLine();

                if (ProcessMetaCmd(text) == false)
                {
                    return;
                }

                _usbController.MoveText(text);
            }
        }

        bool ProcessMetaCmd(string text)
        {
            switch (text)
            {
                case null:
                    _exit = text == null;
                    return false;

                case "--mode":
                    return false;

                case "--shutdown":
                    _usbController.Shutdown();
                    _exit = true;
                    return false;

                case "--exit":
                    _exit = true;
                    return false;
            }

            return true;
        }

        private void KeyboardTest()
        {
            //{
            //    string txt = "abc+*()<ime>xptmxm<ime>";
            //    _usbController.SendText(txt + Environment.NewLine);
            //}

            //{
            //    string txt = "<shift_down>abc<shift_up>";
            //    _usbController.SendText(txt + Environment.NewLine);
            //}

            //{
            //    string txt = "<ctrl_down><esc><ctrl_up>";
            //    _usbController.SendText(txt);
            //}

            //{
            //    string txt = "<capslock>test is good<capslock><return>";
            //    _usbController.SendText(txt);
            //}

            //{
            //    string txt = "<ctrl_down><shift_down><esc><shift_up><ctrl_up>";
            //    _usbController.SendText(txt);
            //}

            while (true)
            {
                Console.Write("Keyboard> ");
                string text = Console.ReadLine();

                if (ProcessMetaCmd(text) == false)
                {
                    return;
                }

                if (text.StartsWith("connect") == true)
                {
                    string[] connectParams = text.Split();
                    _usbController.Connect(connectParams[1], short.Parse(connectParams[2]));
                    continue;
                }

                _usbController.SendText(text);
            }

            //while (true)
            //{
            //    ConsoleKeyInfo key = Console.ReadKey();

            //    _usbController.SendChar(key);
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

            //    _usbController.SendKeyRawBuffer(buf.ToArray());
            //}
        }
    }
}
