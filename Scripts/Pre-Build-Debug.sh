if [[ ! -e "../OpenLyricsClient/bin/Debug/net7.0/IpaDic/" ]]; then
mkdir -p "../OpenLyricsClient/bin/Debug/net7.0/IpaDic/"
cp -r "../Binaries/IpaDic/" "../OpenLyricsClient/bin/Debug/net7.0/IpaDic/"
fi

chmod +x "../OpenLyricsClient/bin/Debug/net7.0/OpenLyricsClient"