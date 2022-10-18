require(HDS);

data = HDS::openStream("T:\project\2022 KD\MALDI Da 50-1000\analysis\H39572-CHCA-1.mzPack");

print(HDS::tree(data));
