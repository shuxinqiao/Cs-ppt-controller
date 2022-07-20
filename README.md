
# C# PowerPoint Controller

A PowerPoint Controller with Window Desktop WPF application, Single HTML remote control page.

## APP Screenshot
![Screenshot](/ScreenShots/Main.PNG)


## Requirement
- .Net FrameWork 4.7.2
- .Net SDK

## Usage

In bin/Debug or bin/Release or Release.zip (if exist).
Run Cs_ppt_controller.exe (as Administrator) file.

Open ppt File and Host Web.

On Web page, input IP addresss as shown on Windows GUI. (e.g. 192.168.0.1)
and submit.

If Page: ; Note: ; connection open shown on page you can control now.

connection closed is WebSocket closed by Windows GUI (or you edited html).

## Possible Error

#### Cannot Host Web / Host Web not working

In C# I use http://*:xxxx/ to listen which requires Run as Administrator.

One fix is by using (3000 is default port)

```
netsh.exe "http add urlacl url=http://*:3000/ user=Everyone"
```

#### Crash when Open PPT file

This is related to Cannot Host Web error. 

If you tried to Run as Administrator, you can Host Web but may 
meet app crash when Open file.

#### Host Web address cannot access

Probably the "Connection-specific DNS Suffix" of your Working IP is not "lan".

Check your IP interfaces by ```ipconfig``` in Windows cmd.

Change the "Connection-specific DNS Suffix" by Google it.
