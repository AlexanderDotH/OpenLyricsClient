@echo off
xcopy /e /k /h /i "..\Binaries\CefBinaries\linux-x64\" "..\OpenLyricsClient\bin\Release\net6.0\publish\linux-x64\CefBinaries\linux-x64\"
xcopy /e /k /h /i "..\Binaries\IpaDic\" "..\OpenLyricsClient\bin\Release\net6.0\publish\linux-x64\IpaDic\"