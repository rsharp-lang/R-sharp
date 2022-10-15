const add = function(x, y) {
	# str(x);
	# str(y);

	t1 = now();
	z = x + y;
	t2 = now();
	
	print(t2 - t1);
	
	z;
}

options(avx_simd = FALSE);

# short vector
a = 1.0: 9000.0;
b = 2.0: 9001.0;

add(a, b);

# long vector
a = 1.0: 80000000.0;
b = 2.0: 80000001.0;

add(a, b);

options(avx_simd = TRUE);

# short vector
a = 1.0: 9000.0;
b = 2.0: 9001.0;

add(a, b);

# long vector
a = 1.0: 80000000.0;
b = 2.0: 80000001.0;

add(a, b);

# old: no SIMD supports
# [1] 00:00:00.0068542
# [1] 00:00:05.2188812
# [1] 00:00:00.0007026
# [1] 00:00:05.4814170

# new: add SIMD supports
# [1] 00:00:00.0043929
# [1] 00:00:00.4820480
# [1] 00:00:00.0001156
# [1] 00:00:00.4234774