# whitespace also could be used as the vector element delimiter
# in the syntax of vector literal [...]
let names = [
    "7-O-Ethylmorroniside 7-氧乙基莫诺苷"
"6''-O-Acetylglycitin 6''-O-乙酰黄豆黄苷"
"Brevifolincarboxylic acid 短叶苏木酚酸"
"Astringin 白皮杉醇葡萄糖苷"
"Angeloylgomisin H 当归酰戈米辛H"
"Cratoxylone 黄牛木酮"
"Cafestol 咖啡醇"
"Dehydrotrametenolic acid 松苓新酸"
"Feretoside 6-β-羟基栀子苷"
"Kahweol 咖啡豆醇"
"Isoastragaloside I 异黄芪皂苷I"
"Purpureaside C 紫地黄苷C"
"Methylnissolin-3-O-glucoside 美迪紫檀苷"
"Sibiricose A6 西伯利亚远志糖A6"
"Trilobatin 三叶苷"
"Tomatine 番茄碱苷"
"Styraxlignolide F"
"(+)-Pinoresinol (+)-松脂酚"
"Tsugaric acid A"
"27-Deoxyactein 27-脱氧升麻亭"
"13-α-(21)-Epoxyeurycomanone 13-α-(21)-环氧宽缨酮"
"5,7,3'-Trihydroxy-6,4',5'-trimethoxyflavone 5,7,3'-三羟基-6,4',5'-三甲氧基黄酮"
"Epimagnolin B 表木兰脂素B"
"EGCG Octaacetate 乙酰化EGCG"
"8-Shogaol 8-姜烯酚"
"6-Demethoxytangeretin 6-去甲氧基桔皮素"
"Eurycomalactone 东革内酯"
"Gypenoside XLVI 七叶胆苷XLVI"
"Eurycomanone 宽缨酮"
"Moluccanin"
"Protohypericin 原金丝桃素"
"Scutellarin methyl ester 灯盏花乙素甲酯"
"Secoxyloganin 断氧化马钱苷"
"Veraguensin 蔚瑞昆森"
"Sinapic acid 芥子酸"
];

print(names);

imports "clr" from "devkit";

const pinyin_lib = clr::open("Microsoft.VisualBasic.Text.GB2312");
# const zh = lapply(names, function(si) clr::call_clr(pinyin_lib, "GetZhFlags", str = si));

# names(zh) = names;

# str(zh);

# for(name in names(zh)) {
#     # print(name);
#     # print(zh[[name]]);

#     flags = zh[[name]];
#     name = strsplit(name);
# }

const split_zh = function(si) {
    si = strsplit(si);
    zh = sapply(si, ci -> any(clr::call_clr(pinyin_lib, "GetZhFlags", str = ci)));
    zh = paste(si[zh], sep = " ");
    en = paste(si[!zh], sep = " ");

    list(zh, en);
}

const names_split = lapply(names, si -> split_zh(si), names = names);

require(JSON);

str(names_split);

names_split
|> JSON::json_encode()
|> writeLines(
    con = `${@dir}/zh_names.json`
)
;