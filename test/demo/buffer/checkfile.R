require(HDS);

let file = HDS::read_stream("F:\MZKit.hdms");
let size = HDS::header_size(file);

print(size);
print(byte_size(size));
