imports "clr" from "devkit";

const lib = clr::open("Microsoft.VisualBasic.Text.GB2312");
const pinyin = clr::call_clr(lib, "TranscriptPinYin", str = "哈喽，大家好呀", sep = " | ");

print(pinyin);