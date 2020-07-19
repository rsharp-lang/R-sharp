imports "ftp" from "R.web";

let cngb as new ftp(server = "ftp.cngb.org");

cngb
:> list.ftp_dirs("/pub/Assembly/GCA/000/001/215/GCA_000001215.4_Release_6_plus_ISO1_MT")
:> print
;