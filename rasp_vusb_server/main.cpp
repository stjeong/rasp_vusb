
#include "stdafx.h"
#include "VUsbServer.h"
#include "IPResolver.h"

#if defined(WIN32)
#else
#include <sys/types.h>
#include <sys/stat.h>
#include <stdlib.h>
#include <stdio.h>
#include <fcntl.h>
#include <unistd.h>
#include <signal.h>
#include <linux/limits.h>
#endif

/*

On raspberry pi:
    # apt install build-essential
    # apt install libbluetooth-dev
    # apt install libssl-dev

*/

int work(bool inConsole)
{
#if defined(WIN32)
    inConsole = true;

    WORD wVersionRequested = MAKEWORD(2, 2);
    WSADATA wsaData;

    ::WSAStartup(wVersionRequested, &wsaData);
#endif

    IPResolver ipResolver;
    VUsbServer usbServer;

    if (ipResolver.open() == false)
    {
        cout << "ipResolver.open failed" << endl;
        return -1;
    }

    ipResolver.startService();

    cout << "ipResolver prepared" << endl;

    if (usbServer.open() == false)
    {
        return -1;
    }

    usbServer.startService();

    cout << "usbServer prepared" << endl;

    if (inConsole == true)
    {
        cout << "Press any key to exit..." << endl;
        string s;
        getline(cin, s);
    }
    else
    {
#if defined(WIN32)
#else
        while (true)
        {
            sleep(1);
        }
#endif
    }

#if defined(WIN32)
    WSACleanup();
#endif

    ipResolver.dispose();
    usbServer.dispose();

	return 0;
}

int main(int argc, char **argv)
{
    bool isConsole = argc >= 2;
#if defined(WIN32)
#else

    if (isConsole == false)
    {
        pid_t pid = fork();
        printf("fork-pid = [%d] \n", pid);

        if (pid < 0)
        {
            printf("(pid < 0) exiting...\n");
            exit(0);
        }
        else if (pid > 0)
        {
            printf("(pid > 0) exiting...\n");
            exit(0);
        }
        
        printf("(pid == 0) working...\n");

        signal(SIGHUP, SIG_IGN);

        printf("signalled...\n");

        //for (int i = 0; i < NR_OPEN; i++)
        //{
        //    close(i);
        //}

        open("/dev/null", O_RDWR);
        if (dup(0) < 0)
        {
            exit(0);
        }

        if (dup(0) < 0)
        {
            exit(0);
        }

        if (chdir("/") < 0)
        {
            exit(0);
        }

        setsid();
    }

#endif

    printf("starting...\n");
    work(isConsole);
}