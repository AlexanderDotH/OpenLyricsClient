@echo off
xcopy /e /k /h /i "..\Binaries\CefBinaries\win-x64\" "..\OpenLyricsClient\bin\Release\net6.0\publish\windows-x64\CefBinaries\win-x64\"
xcopy /e /k /h /i "..\Binaries\IpaDic\" "..\OpenLyricsClient\bin\Release\net6.0\publish\windows-x64\IpaDic\"