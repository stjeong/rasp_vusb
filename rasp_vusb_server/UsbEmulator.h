#pragma once

#include "NoResetEvent.h"
#include "QueueTS.h"
#include "KeyboardDevice.h"
#include "MouseDevice.h"

#include <thread>
using namespace std;

class UsbEmulator
{
public:
    UsbEmulator();
    ~UsbEmulator();

    void Enqueue(char *buffer, int bufLen);
    void Run();
    void Open();
    void Close();

    class QueueItem
    {
    public:
        char *item;
        int itemLen;

        QueueItem()
        {
            item = nullptr;
            itemLen = 0;
        }

        void Dispose()
        {
            if (item != nullptr)
            {
                delete[] item;
                item = nullptr;
            }
        }
    };

private:
    void ProcessUsbInput(QueueItem buf);
        
    QueueTS<QueueItem> _queue;
    NoResetEvent _event;

    KeyboardDevice _keyboard;
    MouseDevice _mouse;

    int _fd;
};

