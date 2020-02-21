str(file.info(!script$fullName));

# get by name
print("list get value by name");

print(file.info(!script$fullName)$Extension);
print((file.info(!script$fullName))$Extension);
print(file.info(!script$fullName)[["Extension"]]);
print((file.info(!script$fullName))[["Extension"]]);

print("list subset by given names");

# list subset
str(file.info(!script$fullName)["Extension"]);
str((file.info(!script$fullName))["Extension"]);

str(file.info(!script$fullName)[["Extension", "Name", "Length"]]);