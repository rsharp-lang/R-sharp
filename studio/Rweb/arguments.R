# print all url query argument values 

for(name in ls()) {
	print(`GET '${name}':`);
	print(get(name));
}