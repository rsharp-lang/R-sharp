require(HDS);

data = HDS::openStream("D:\biodeep\biodeepdb_v3\biodeepdb_v3\data\biodeepdb_v3.pack");

print(HDS::tree(data));
