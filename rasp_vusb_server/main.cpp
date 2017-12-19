
#include "stdafx.h"
#include "VUsbServer.h"
#include "IPResolver.h"

#include <sys/types.h>
#include <sys/stat.h>
#include <stdlib.h>
#include <stdio.h>
#include <fcntl.h>
#include <unistd.h>
#include <signal.h>
#include <linux/limits.h>

int work(bool inConsole)
{
    IPResolver ipResolver;
    VUsbServer usbServer;

    if (ipResolver.open() == false)
    {
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
        while (true)
        {
            sleep(1);
        }
    }

    ipResolver.dispose();
    usbServer.dispose();

	return 0;
}

int main(int argc, char **argv)
{
    bool isConsole = argc >= 2;

    if (isConsole == false)
    {
        pid_t pid = fork();
        printf("pid = [%d] \n", pid);

        if (pid < 0)
        {
            exit(0);
        }
        else if (pid > 0)
        {
            exit(0);
        }

        signal(SIGHUP, SIG_IGN);

#if !defined(_NR_OPEN)
#define NR_OPEN	1024
#endif

        for (int i = 0; i < NR_OPEN; i++)
        {
            close(i);
        }

        open("/dev/null", O_RDWR);
        dup(0);
        dup(0);

        chdir("/");

        setsid();
    }

    work(isConsole);
}