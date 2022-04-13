options(strict = FALSE);


a = list(a = 1,b=2,c=3333);

if ("list" == (modeof a)) {
	print("yes");
} else {
	stop();
}