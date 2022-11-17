if [[ ! -e "../OpenLyricsClient/bin/Debug/net6.0/CefBinaries/linux-x64/" ]]; then
mkdir -p "../OpenLyricsClient/bin/Debug/net6.0/CefBinaries/linux-x64/"
cp -r "../Binaries/CefBinaries/linux-x64/" "../OpenLyricsClient/bin/Debug/net6.0/CefBinaries/" 
fi

if [[ ! -e "../OpenLyricsClient/bin/Debug/net6.0/IpaDic/" ]]; then
mkdir -p "../OpenLyricsClient/bin/Debug/net6.0/IpaDic/"
cp -r "../Binaries/IpaDic/" "../OpenLyricsClient/bin/Debug/net6.0/IpaDic/"
fi

if [[ ! -e "../OpenLyricsClient/bin/Debug/net6.0/icudtl.dat" ]]; then
mkdir -p "../OpenLyricsClient/bin/Debug/net6.0/"
cp "../Binaries/CefBinaries/icudtl.dat" "../OpenLyricsClient/bin/Debug/net6.0/icudtl.dat"
fi

chmod +x "../OpenLyricsClient/bin/Debug/net6.0/OpenLyricsClient"