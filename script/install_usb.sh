#!/bin/bash

# In order to run this script, make install_usb.sh runnable with "sudo chmod +x /share/install_usb.sh"
# then run "sudo /share/install_usb.sh"

if cat /boot/config.txt | grep dtoverlay=dwc2; then
echo exists "dtoverlay=dwc2"
else
echo "dtoverlay=dwc2" | sudo tee -a /boot/config.txt
echo "dwc2" | sudo tee -a /etc/modules
fi

sudo chmod +x /share/triple_usb_device.sh
sudo chmod +x /share/rasp_vusb_server.out
sudo chmod +x /share/uninstall_usb.sh

if systemctl | grep create-triple-usb; then
echo exists "create-triple-usb"
else
sudo cp /share/create-triple-usb.service /etc/systemd/system/
sudo systemctl enable create-triple-usb
fi

if systemctl | grep usb_server; then
echo exists "usb_server"
else
sudo cp /share/usb_server.service /etc/systemd/system/
sudo systemctl enable usb_server
fi

sudo systemctl daemon-reload
