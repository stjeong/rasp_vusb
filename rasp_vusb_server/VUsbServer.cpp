
#include "stdafx.h"
#include "VUsbServer.h"

VUsbServer::VUsbServer()
{
    _sock = 0;
    _emulator.Run();
}

VUsbServer::~VUsbServer()
{
    dispose();
}

void VUsbServer::dispose()
{
#if defined(WIN32)
    ::closesocket(_sock);
#else
    if (_sock != 0)
    {
        ::close(_sock);
    }
#endif

    _sock = 0;

    _emulator.Close();
}

bool VUsbServer::open()
{
    _emulator.Open();

#if defined(WIN32)
    _sock = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
#else
    _sock = socket(AF_INET, SOCK_STREAM, 0);
#endif
    if (_sock < 0) {
        perror("VUsbServer::open - sock error\n");
        return false;
    }

    int addr_len = sizeof(struct sockaddr_in);

    memset((void*)&_server_addr, 0, addr_len);
    _server_addr.sin_family = AF_INET;
    _server_addr.sin_addr.s_addr = htonl(INADDR_ANY);
    _server_addr.sin_port = htons((PORT + 1));

    int ret = ::bind(_sock, (struct sockaddr*)&_server_addr, addr_len);
    if (ret < 0) {
        perror("VUsbServer::open - bind error\n");
        return false;
    }

    ::listen(_sock, 5);

    return true;
}

bool VUsbServer::ReadData(int socket, char *buf, int len)
{
    int totalRead = len;

    while (totalRead > 0)
    {
        int readBytes = recv(socket, buf, len, 0);
        if (readBytes <= 0)
        {
            break;
        }

        totalRead -= readBytes;
    }

    if (totalRead == 0)
    {
        return true;
    }

    return false;
}

void VUsbServer::startService()
{
    thread t([&]()
    {
        struct sockaddr_in server_socket;
        socklen_t addr_len = sizeof(server_socket);

        while (true)
        {
            int clntSocket = accept(_sock, (struct sockaddr *)&server_socket, &addr_len);

            thread clntThread([this](int connectedSocket, UsbEmulator &emulator)
            {
                char buf[4096];

                while (true)
                {
                    memset(buf, 0, sizeof(buf));

                    // get length of packet
                    bool result = ReadData(connectedSocket, buf, 4);
                    if (result == false)
                    {
                        break;
                    }

                    int packetLen = *(int *)buf;

                    if (packetLen > 4096)
                    {
                        ReadData(connectedSocket, buf, packetLen);
                        printf("dropped long packet = size(%d)\n", packetLen);
                        break;
                    }

                    result = ReadData(connectedSocket, buf, packetLen);
                    if (result == false)
                    {
                        break;
                    }

                    printf("recvBytes == %d\n", packetLen);

                    if (packetLen <= 0)
                    {
                        break;
                    }

                    emulator.Enqueue(buf, packetLen);
                }

                if (connectedSocket > 0)
                {
#if defined(WIN32)
                    ::closesocket(connectedSocket);
#else
                    ::close(connectedSocket);
#endif
                }
                
            }, clntSocket, std::ref(_emulator));

            clntThread.detach();
        }

    });

    t.detach();
}