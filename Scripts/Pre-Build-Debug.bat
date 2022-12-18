@echo off
xcopy /e /k /h /i "..\Binaries\CefBinaries\win-x64\" "..\OpenLyricsClient\bin\Debug\net6.0\CefBinaries\win-x64\" /Y /D
xcopy /e /k /h /i "..\Binaries\IpaDic\" "..\OpenLyricsClient\bin\Debug\net6.0\IpaDic\" /Y /D
copy "..\Binaries\CefBinaries\icudtl.dat" "..\OpenLyricsClient\bin\Debug\net6.0\icudtl.dat"
