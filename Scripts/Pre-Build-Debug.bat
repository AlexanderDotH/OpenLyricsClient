@echo off
taskkill /f /im OpenLyricsClient.exe
xcopy /e /k /h /i "..\Binaries\IpaDic\" "..\OpenLyricsClient\bin\Debug\net6.0\IpaDic\" /Y /D
xcopy /e /k /h /i "..\Binaries\IpaDic\" "..\OpenLyricsClient\bin\Debug\net7.0\IpaDic\" /Y /D
