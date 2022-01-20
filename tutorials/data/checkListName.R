l = list(aaa = 1, bbb = 2);

if ("aaa" in l) {
	print(1);
}

if ("ccc" in l) {
	print(2);
} else {
	print("no ccc");
}

d = data.frame(aaa = [2,42,3,4,666], check = TRUE);

print(d);

if ("aaa" in d) {
	print("aaaaaaaaaaa");
}

if ("xxxxx" in d) {
	print("wrong!");
} else {
	print("no xxxxx");
}