
#include "stdafx.h"
#include "MouseDevice.h"

MouseDevice::MouseDevice()
{
}

MouseDevice::~MouseDevice()
{
}

void MouseDevice::SendRelative(int fd, char *mouseInput, int keyLen)
{
    if (fd <= 0)
    {
        perror("rel mouse: not opened");
        return;
    }

    for (int i = 0; i < keyLen; i++)
    {
        printf("rel mouse %d\n", mouseInput[i]);
    }

    printf("\n");

    mouse_rel_report_t item;
    int expected = sizeof(mouse_rel_report_t);

    printf("rel mouse report len == %d\n", expected);

    item.report_id = 2;
    for (int i = 0; i < keyLen; i += 3)
    {
        item.buttons = mouseInput[i];
        item.x = mouseInput[i + 1];
        item.y = mouseInput[i + 2];

        printf("rel mouse: buttons %d, x = %d, y = %d\n", item.buttons, item.x, item.y);

        if (write(fd, &item, expected) != expected)
        {
            perror("failed to rel-mouse press");
            return;
        }
    }
}

void MouseDevice::SendAbsolute(int fd, char *mouseInput, int keyLen)
{
    if (fd <= 0)
    {
        perror("abs mouse: not opened");
        return;
    }

    for (int i = 0; i < keyLen; i++)
    {
        printf("abs mouse %d\n", mouseInput[i]);
    }

    printf("\n");

    mouse_abs_report_t item;
    int expected = sizeof(mouse_abs_report_t);

    printf("abs mouse report len == %d\n", expected);

    item.report_id = 3;
    for (int i = 0; i < keyLen; i += 5)
    {
        item.x = (short)((mouseInput[i + 0]) | (mouseInput[i + 1] << 8));
        item.y = (short)((mouseInput[i + 2]) | (mouseInput[i + 3] << 8));
        item.wheel = mouseInput[i + 4];

        printf("abs mouse: x = %d, y = %d, wheel %d\n", item.x, item.y, item.wheel);

        if (write(fd, &item, expected) != expected)
        {
            perror("failed to abs-mouse press");
            return;
        }
    }
}