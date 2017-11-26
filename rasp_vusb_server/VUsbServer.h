#pragma once

#if defined(WIN32)
#include <winsock2.h>
typedef int socklen_t;
#else
#include <pthread.h>
#include <arpa/inet.h>
#include <sys/socket.h>
#include <sys/types.h>
#include <sys/select.h>
#include <unistd.h>
#endif

#include <stdio.h>
#include <assert.h>
#include <stdlib.h>
#include <string.h>

#include <iostream>
#include <string>

// On raspberry pi zero,
// sudo apt-get install g++-4.8 
#include <thread>

#include "UsbEmulator.h"

using namespace std;

class VUsbServer
{
public:
    VUsbServer();
    ~VUsbServer();

    bool open();
    void startService();
    void dispose();

private:
    int _sock;
    sockaddr_in _server_addr;
    UsbEmulator _emulator;

    bool ReadData(int socket, char *buf, int len);
};

