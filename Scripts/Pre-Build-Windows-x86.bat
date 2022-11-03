@echo off
xcopy /e /k /h /i "..\Binaries\CefBinaries\win-x86\" "..\OpenLyricsClient\bin\Release\net6.0\publish\windows-x86\CefBinaries\win-x86\"
xcopy /e /k /h /i "..\Binaries\IpaDic\" "..\OpenLyricsClient\bin\Release\net6.0\publish\windows-x86\IpaDic\"
copy "..\Binaries\CefBinaries\icudtl.dat" "..\OpenLyricsClient\bin\Release\net6.0\publish\windows-x86\icudtl.dat"