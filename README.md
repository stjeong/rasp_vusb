What is it?
================================
This repo explains how to turn your Raspberry Pi Zero into USB Keyboard and Mouse. Also provides sample code and binaries to control them.

Steps - Installation
================================

1. Flash your SD card with NOOBS (download: https://www.raspberrypi.org/downloads/noobs/) or use your installed Raspbian.

2. Make /share directory on your raspberry pi.
    ```
    $ sudo mkdir -m 1777 /share
    ```

3. Run /script/deploy_with_pscp.bat on your PC to deploy files to Raspberry PI Zero. (download pscp.exe from https://www.chiark.greenend.org.uk/~sgtatham/putty/latest.html)
    ```
    c:\...\script> deploy_with_pscp.bat [your_raspberrypi_ip]

    For example, if the address of your raspberry pi is 192.168.0.100

    c:\...\script> deploy_with_pscp.bat 192.168.0.100
    ```

4. On your raspberry pi, make /share/install_usb.sh and /share/rasp_vusb_server.out runnable.
    ```
    $ sudo chmod +x /share/install_usb.sh
    $ sudo chmod +x /share/rasp_vusb_server.out
    ```

5. Run install_usb.sh
    ```
    $ sudo /share/install_usb.sh
    ```

6. That's all, all of the changes will be applied after reboot.
    ```
    $ sudo reboot
    ```

7. Finally, connect your Raspberry PI zero to your Computer. Go to your "Control panel" / "Device Manager", you can find 3 new devices.
    ```
    Human Interface Devices
        * USB Input Device

    Keyboards
        * HID Keyboard Device

    Mice and other pointing devices
        * HID-compliant mouse  (Absolute position + buttons)
        * HID-compliant mouse  (Relative position + wheel)
    ```

    You can get the report descriptors at /report_descriptors.txt

How to Test
================================
Now, you can run "InputController.exe (from /bin/v1_0_0_2/InputController.zip)" on your Windows PC. As soon as run, it will find "usb_server" program which is run in Raspberry PI and connect it automatically.

At first, InputController parse input as mouse data. So you can move to specific position as whatever you want but need to calculate for your circumstances. If you move to x = 50, y = 100 and your monitor's resolution is 1920 * 1080, your input has to be like this,

```
x = 50 * 32767 / 1920 = 853
y = 100 * 32767 / 1080 = 3033
```

then type it,

```
853 3033
```

you can find your mouse position is moved to (50, 100) on windows.

Also, you can move your mouse relatively. For this, type '+' or '-' prefix to number.

```
+50 -10
```

And control wheel on the mouse with 'w' prefix to offset value.

```
w10
```

It acts as scrolling down, or type "w-50" to scroll up with offset 50.

For testing input as keyboard, change the mode by typing "--mode" command,

```
--mode
```
(If you type "--mode" again, it will change to mouse input mode)

Now you can type any text and just hit ENTER,

```
test is good
```

then, your PC will accept "test is good" key inputs. Of course, you can send any special inputs of these,

```
Left Window key: <window>
IME toggle key: <ime>
Enter Key: <return>
Control Down: <ctrl_down>
Control Up: <ctrl_up>
Shift Down: <shift_down>
Shift Up: <shift_up>
Alt Down: <alt_down>
Alt Up: <alt_up>
CapsLock: <capslock>
ESC: <esc>
Backspace: <backspace>
Tab: <tab>
Insert: <insert>
Home: <home>
Page Up: <pageup>
Page Down: <pagedown>
Delete: <del>
End: <end>
Left Arrow key: <left>
Right Arrow key: <right>
Up Arrow key: <up>
Down Arrow Key: <down>
F1 ~ F12: <f1> ~ <f12>
```

If you type like this,

```
test is good<backspace>d <shift_down>wow<shift_up>
```

you will see this text input,

```
test is good WOW
```


Steps - Uninstallation
================================
1. Just run uninstall_usb.sh.

```
$ sudo /share/uninstall_usb.sh
``` 

2. Reboot.
```
$ sudo reboot
``` 


Change Log
================================
1.0.0.2 - Dec 20, 2017

* apply ssl socket


1.0.0.1 - Oct 13, 2017

* Add "--shutdown" command to shutdown raspberry pi.


1.0.0.0 - Oct 12, 2017

* Initial checked-in


How to build
================================
If you want to modify InputController.exe and rasp_vusb_server.out, just load rasp_usb.sln in Visual Studio 2017, and build it.

It needs connection info to Raspberry Pi for compiling C++ source codes to ARM machine codes.

Requests or Contributing to Repository
================================
Any help and advices for this repo are welcome.

License
================================
Apache License V2.0
(Refer to LICENSE.txt)