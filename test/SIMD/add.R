const add = function(x, y) {
	t1 = now();
	z = x + y;
	t2 = now();
	
	print(t2 - t1);
	
	z;
}

options(avx_simd = FALSE);

# short vector
a = 1: 6000;
b = 2: 6001;

add(a, b);

# long vector
a = 1: 100000000;
b = 2: 100000001;

add(a, b);

options(avx_simd = TRUE);

# short vector
a = 1: 6000;
b = 2: 6001;

add(a, b);

# long vector
a = 1: 100000000;
b = 2: 100000001;

add(a, b);
