
#pragma once

#if defined(WIN32)
#else
#include <pthread.h>
#include <fcntl.h>
#include <unistd.h>
#endif

#include <stdint.h>
#include <inttypes.h>

#include <string.h>
#include <stdio.h>
#include <ctype.h>
#include <errno.h>
#include <stdio.h>
#include <stdlib.h>

#pragma pack(1)
struct mouse_rel_report_t
{
    uint8_t report_id;
    uint8_t buttons;
    int8_t x;
    int8_t y;
};

#pragma pack(1)
struct mouse_abs_report_t
{
    uint8_t report_id;
    int16_t x;
    int16_t y;
    int8_t wheel;
};

class MouseDevice
{
public:
    MouseDevice();
    ~MouseDevice();

    void SendRelative(int fd, char *keyInput, int keyLen);
    void SendAbsolute(int fd, char *keyInput, int keyLen);
};

