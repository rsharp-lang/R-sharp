imports "stringr" from "R.base";

let input as string = "/mnt/smb2/project/<e4><b8><8a><e6><b5><b7><e6><b4><be><e6><a3><ae><e8><af><ba><e7><94><9f><e7><89><a9><e7><a7><91><e6><8a><80><e8><82><a1><e4><bb><bd><e6><9c><89><e9><99><90><e5><85><ac><e5><8f><b8>/<e5><8e><9f><e5><a7><8b><e6><95><b0><e6><8d><ae>/<e5><85><a8><e9><89><b4><e5><ae><9a>/pos/flavone_test20200807/save/AnnoDataSet.mgf";

print(decode.R_rawstring(input, encoding = "utf8"));