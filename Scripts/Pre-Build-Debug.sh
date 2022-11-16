mkdir -p "../OpenLyricsClient/bin/Debug/net6.0/CefBinaries/linux-x64/"
cp -r "../Binaries/CefBinaries/linux-x64/" "../OpenLyricsClient/bin/Debug/net6.0/CefBinaries/" 

mkdir -p "../OpenLyricsClient/bin/Debug/net6.0/IpaDic/"
cp -r "../Binaries/IpaDic/" "../OpenLyricsClient/bin/Debug/net6.0/IpaDic/"

mkdir -p "../OpenLyricsClient/bin/Debug/net6.0/"
cp "../Binaries/CefBinaries/icudtl.dat" "../OpenLyricsClient/bin/Debug/net6.0/icudtl.dat"
