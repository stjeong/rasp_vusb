
#include "stdafx.h"
#include "IPResolver.h"

IPResolver::IPResolver()
{
    _sock = 0;
}

IPResolver::~IPResolver()
{
    dispose();
}

void IPResolver::dispose()
{
    if (_sock != 0)
    {
        ::close(_sock);
    }

    _sock = 0;
}

void IPResolver::startService()
{
    thread t([&]()
    {
        fd_set readfd;
        char buffer[1024];

        while (true) 
        {
            FD_ZERO(&readfd);
            FD_SET(_sock, &readfd);
            struct sockaddr_in client_addr;
            socklen_t addr_len = sizeof(client_addr);

            int ret = select(_sock + 1, &readfd, NULL, NULL, 0);
            if (ret > 0) 
            {
                if (FD_ISSET(_sock, &readfd)) 
                {
                    memset(buffer, 0, sizeof(buffer));
                    ssize_t recvByets = recvfrom(_sock, buffer, 1024, 0, (struct sockaddr*)&client_addr, &addr_len);

                    if (recvByets != 0 && strcmp(buffer, IP_FOUND) == 0)
                    {
                        printf("\nClient connection information:\n\t IP: %s, Port: %d\n",
                            inet_ntoa(client_addr.sin_addr), ntohs(client_addr.sin_port));

                        memcpy(buffer, IP_FOUND_ACK, strlen(IP_FOUND_ACK) + 1);
                        sendto(_sock, buffer, strlen(IP_FOUND_ACK) + 1, 0, (struct sockaddr*)&client_addr, addr_len);
                    }
                }
            }
        }
    });

    t.detach();
}

bool IPResolver::open()
{
    _sock = socket(AF_INET, SOCK_DGRAM, 0);

    if (_sock < 0) {
        perror("IPResolver::open - sock error\n");
        return false;
    }

    int addr_len = sizeof(struct sockaddr_in);

    memset((void*)&_server_addr, 0, addr_len);
    _server_addr.sin_family = AF_INET;
    _server_addr.sin_addr.s_addr = htonl(INADDR_ANY);
    _server_addr.sin_port = htons(PORT);

    int ret = ::bind(_sock, (struct sockaddr*)&_server_addr, addr_len);
    if (ret < 0) {
        perror("IPResolver::open - bind error\n");
        return false;
    }

    return true;
}