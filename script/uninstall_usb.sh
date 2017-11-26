#!/bin/bash

sudo rm -r /sys/kernel/config/usb_gadget/g1/configs/c.1/

sudo systemctl disable create-triple-usb
sudo rm /etc/systemd/system/create-triple-usb.service

sudo systemctl disable usb_server
sudo rm /etc/systemd/system/usb_server.service

sudo chmod -x /share/triple_usb_device.sh
sudo chmod -x /share/rasp_vusb_server.out
