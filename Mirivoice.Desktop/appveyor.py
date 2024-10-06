import os
import sys
from datetime import datetime

appcast_ver = os.environ.get('VERSION')


def write_appcast(appcast_os, appcast_rid, appcast_file):

    xml = '''<?xml version="1.0" encoding="utf-8"?>
<rss version="2.0" xmlns:sparkle="http://www.andymatuschak.org/xml-namespaces/sparkle">
<channel>
    <title>MiriVoice</title>
    <language>en</language>
    <item>
    <title>%s</title>
    <pubDate>%s</pubDate>
    <enclosure url="https://github.com/EX3exp/MiriVoice/releases/download/%s/%s"
                sparkle:version="%s"
                sparkle:shortVersionString="%s"
                sparkle:os="%s"
                type="application/octet-stream"
                sparkle:signature="" />
    </item>
</channel>
</rss>''' % (appcast_ver, datetime.now().strftime("%a, %d %b %Y %H:%M:%S %z"),
             appcast_ver, appcast_file, appcast_ver, appcast_ver, appcast_os)

    with open("appcast.%s.xml" % (appcast_rid), 'w') as f:
        f.write(xml)

def write_info_plist():
    plist = '''<?xml version="1.0" encoding="utf-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
  <dict>
    <key>CFBundleName</key>
    <string>MiriVoice</string>
    <key>CFBundleDisplayName</key>
    <string>MiriVoice</string>
    <key>CFBundleIdentifier</key>
    <string>com.ex3exp.mirivoice</string>
    <key>CFBundleVersion</key>
    <string>@@</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleSignature</key>
    <string>????</string>
    <key>CFBundleExecutable</key>
    <string>MiriVoice</string>
    <key>CFBundleIconFile</key>
    <string>mirivoice.icns</string>
    <key>NSPrincipalClass</key>
    <string>NSApplication</string>
    <key>NSHighResolutionCapable</key>
    <true />
  </dict>
</plist>'''.replace('@@', appcast_ver)
    
    with open("Mirivoice.Desktop/osxbuild/MiriVoice.app/Contents/Info.plist", 'w') as f:
        f.write(plist)

if sys.platform == 'win32':

    os.system("dotnet restore Mirivoice.Desktop/Mirivoice.Desktop.csproj -r win-x86")
    os.system(
        "dotnet publish Mirivoice.Desktop/Mirivoice.Desktop.csproj -c Release -r win-x86 -o Mirivoice.Desktop/bin/win-x86 -p:AssemblyVersion=%s" % (appcast_ver))
    write_appcast("windows", "win-x86", "MiriVoice-win-x86.zip")

    os.system("dotnet restore Mirivoice.Desktop/Mirivoice.Desktop.csproj -r win-x64")
    os.system(
        "dotnet publish Mirivoice.Desktop/Mirivoice.Desktop.csproj -c Release -r win-x64 -o Mirivoice.Desktop/bin/win-x64 -p:AssemblyVersion=%s" % (appcast_ver))
    write_appcast("windows", "win-x64", "MiriVoice-win-x64.zip")


elif sys.platform == 'darwin':
    os.system("git checkout Mirivoice.Desktop/Mirivoice.Desktop.csproj")
    os.system(
        "sed -i '' \"s/0.0.0/%s/g\" Mirivoice.Desktop/Mirivoice.Desktop.csproj" % (appcast_ver))
    write_info_plist()
    os.system("dotnet restore Mirivoice.Desktop/Mirivoice.Desktop.csproj -r osx-x64")
    os.system("dotnet publish Mirivoice.Desktop/Mirivoice.Desktop.csproj -r osx-x64 -c Release -o Mirivoice.Desktop/bin/osx-x64 -p:AssemblyVersion=%s" % (appcast_ver))
    os.system("cp -a Mirivoice.Desktop/bin/osx-x64/ Mirivoice.Desktop/osxbuild/MiriVoice.app/Contents/MacOS/")
    os.system("chmod +x Mirivoice.Desktop/osxbuild/MiriVoice.app/run.sh")
    os.system("ditto -c -k --keepParent Mirivoice.Desktop/osxbuild/MiriVoice.app Mirivoice.Desktop/osxbuild/MiriVoice-osx-x64.zip")
    os.system("git checkout Mirivoice.Desktop/Mirivoice.Desktop.csproj")

    write_appcast("macos", "osx-x64", "MiriVoice-osx-x64.zip")

else:
    os.system("dotnet restore Mirivoice.Desktop/Mirivoice.Desktop.csproj -r linux-x64")
    os.system(
        "dotnet publish Mirivoice.Desktop/Mirivoice.Desktop.csproj -c Release -r linux-x64 -o Mirivoice.Desktop/bin/linux-x64 -p:AssemblyVersion=%s" % (appcast_ver))
    os.system("chmod +x Mirivoice.Desktop/bin/linux-x64/MiriVoice")
    os.system("tar -C Mirivoice.Desktop/bin/linux-x64 -czvf MiriVoice-linux-x64.tar.gz .")
    write_appcast("linux", "linux-x64", "MiriVoice-linux-x64.tar.gz")