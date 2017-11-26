
#include "stdafx.h"
#include "UsbEmulator.h"

UsbEmulator::UsbEmulator() : _queue(10)
{
}

UsbEmulator::~UsbEmulator()
{
}

void UsbEmulator::Open()
{
#if defined(WIN32)
#else
    char *filename = "/dev/hidg0";

    int fd = open(filename, O_RDWR, 0666);
    if (fd == -1)
    {
        perror(filename);
        return;
    }

    printf("keyboard/mouse opened\n");
    _fd = fd;
#endif
}

void UsbEmulator::Close()
{
#if defined(WIN32)
#else
    if (_fd <= 0)
    {
        return;
    }

    close(_fd);
    _fd = 0;
#endif
}

void UsbEmulator::Enqueue(char *buffer, int bufLen)
{
    QueueItem input;

    input.item = new char[bufLen];
    memcpy(input.item, buffer, bufLen);
    input.itemLen = bufLen;

    _queue.Add(input);

    _event.Set();
}

void UsbEmulator::Run()
{
    thread clntThread([&]()
    {
        while (true)
        {
            _event.WaitOne();

            while (_queue.Size() != 0)
            {
                QueueItem input = _queue.Get();
                ProcessUsbInput(input);
                input.Dispose();
            }

        }
    });

    clntThread.detach();
}

void UsbEmulator::ProcessUsbInput(QueueItem buf)
{
    if (buf.item == nullptr || buf.itemLen == 0)
    {
        return;
    }

    char cmdByte = buf.item[0];

    if (cmdByte == KEYBOARD_INPUT_CMD)
    {
        _keyboard.Send(_fd, buf.item + 1, buf.itemLen - 1);
    }
    else if (cmdByte == REL_MOUSE_INPUT_CMD)
    {
        _mouse.SendRelative(_fd, buf.item + 1, buf.itemLen - 1);
    }
    else if (cmdByte == ABS_MOUSE_INPUT_CMD)
    {
        _mouse.SendAbsolute(_fd, buf.item + 1, buf.itemLen - 1);
    }
}