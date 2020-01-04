#pragma once

#if defined(WIN32)
#include <winsock2.h>
typedef int socklen_t;
#else

#include <errno.h>
#include <bluetooth/bluetooth.h>
#include <bluetooth/sdp.h>
#include <bluetooth/sdp_lib.h>
#include <bluetooth/rfcomm.h>
#include <sys/wait.h>
#include <signal.h>

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

#include <thread>

using namespace std;


// https://raw.githubusercontent.com/pyang30/linux-udp-broadcast-example/master/b_server.c

class BluetoothResolver
{
public:
    BluetoothResolver();
    ~BluetoothResolver();

    bool open();
    void startService();
    void dispose();

private:
    int _sock;
    struct sockaddr_in _server_addr;
};

