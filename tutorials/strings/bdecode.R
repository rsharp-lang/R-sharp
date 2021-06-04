const bstr = 'd8:announce41:http://bttracker.debian.org:6969/announce7:comment35:"Debian CD from cdimage.debian.org"13:creation datei1612616380e9:httpseedsl146:https://cdimage.debian.org/cdimage/release/edu//srv/cdbuilder.debian.org/dst/deb-cd/weekly-builds/amd64/iso-cd/debian-edu-10.8.0-amd64-netinst.iso146:https://cdimage.debian.org/cdimage/archive/edu//srv/cdbuilder.debian.org/dst/deb-cd/weekly-builds/amd64/iso-cd/debian-edu-10.8.0-amd64-netinst.isoe4:infod6:lengthi425721856e4:name35:debian-edu-10.8.0-amd64-netinst.iso12:piece lengthi262144e6:piecesi32480eee';

print("Bencoded string:");
print(bstr);

cat("\n");

str(bdecode(bstr));