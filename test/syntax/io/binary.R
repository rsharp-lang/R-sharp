require(buffer);

using smp as file("F:\P210702367.SMP") {
	
	const buffer = smp[64:(1024*200)];
	const data   = buffer[buffer != 0];
	const chars  = string(data[(data >= 32) && (data < 128)], encoding = "GB2312");
	
	print(chars);
}