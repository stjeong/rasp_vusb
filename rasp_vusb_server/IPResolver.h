#pragma once

#include <pthread.h>
#include <arpa/inet.h>
#include <sys/socket.h>
#include <sys/types.h>
#include <sys/select.h>
#include <unistd.h>

#include <stdio.h>
#include <assert.h>
#include <stdlib.h>
#include <string.h>

#include <iostream>
#include <string>

// sudo apt-get install g++-4.8
#include <thread>

using namespace std;


// https://raw.githubusercontent.com/pyang30/linux-udp-broadcast-example/master/b_server.c

class IPResolver
{
public:
    IPResolver();
    ~IPResolver();

    bool open();
    void startService();
    void dispose();

private:
    int _sock;
    struct sockaddr_in _server_addr;
};

