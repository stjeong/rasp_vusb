
#pragma once

#if defined(WIN32)

#else
#include <pthread.h>
#include <fcntl.h>
#include <unistd.h>
#endif

#include <string.h>
#include <stdio.h>
#include <ctype.h>
#include <errno.h>
#include <stdio.h>
#include <stdlib.h>

class KeyboardDevice
{
public:
    KeyboardDevice();
    ~KeyboardDevice();

    void Send(int fd, char *keyInput, int keyLen);
};

