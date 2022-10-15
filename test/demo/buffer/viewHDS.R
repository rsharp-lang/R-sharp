require(HDS);

data = HDS::openStream("F:\5-livers.mzPack");

print(HDS::tree(data));
