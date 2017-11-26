
#include "stdafx.h"
#include "KeyboardDevice.h"

KeyboardDevice::KeyboardDevice()
{
}

KeyboardDevice::~KeyboardDevice()
{
}

void KeyboardDevice::Send(int fd, char *keyInput, int keyLen)
{
#if defined(WIN32)

#else
    if (fd <= 0)
    {
        perror("keyboard: not opened");
        return;
    }

    for (int i = 0; i < keyLen; i++)
    {
        printf("keyboard %d\n", keyInput[i]);
    }

    printf("\n");

    char keyPress[8] = { 0 };

    for (int i = 0; i < keyLen; i += 2)
    {
        keyPress[0] = 1;
        keyPress[1] = keyInput[i];
        keyPress[3] = keyInput[i + 1];

        int result = write(fd, keyPress, 8);
        if (result != 8)
        {
            printf("result == %d\n", result);
            perror("failed to key press");
            return;
        }
    }
#endif
}