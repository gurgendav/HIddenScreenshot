# HIddenScreenshot
A simple program which allows to take screenshots and send them via email without any notice just buy pressing hotkey

## Setup

### 1) Edit `appconfig.json`

There is `appconfig.json` file next to the single executable

Open that file in any text editor and configure.

`ToEmail` - Email to which you want to receive made screenshots

`UserName` - Username of SMTP Server from which screenshot will be sent (by default "hiddenscreenshot@gmail.com")

`Password` - Password of SMTP Server from which screenshot will be sent (by default "hiddenscreenshot^123")

`SmtpHost` - Hostname of SMTP Server from which screenshot will be sent (by default "smtp.gmail.com")

`SmtpPort` - Port of SMTP Server from which screenshot will be sent (by default "587")

### 2) Execute the application

If there were any problem with setting up the hotkeys or loading configuration application will show message window with error.
In success case no any kind of visual feedback will be given. It will just run in the background and wait for your screenshots.

## Take the screenshots

To take screenshots while application is running just press `Ctrl + F11` and after few seconds you should get your brand new screenshot at email you setup as `ToEmail`.
Once you are done press `Ctrl + Shift + F12` and that will exit the application.
