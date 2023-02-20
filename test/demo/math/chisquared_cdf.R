print(chi_square(x = 2.0,	freedom = 3.0)); # ~0.428
print(chi_square(x = 1.0,	freedom = 0.5)); # ~0.846
print(chi_square(x = -1.0,	freedom = 4.0)); # 0.0
print(chi_square(x = 2.0,	freedom = -1.0)); # NA


cdf = function(x, k) {
	gamma.cdf(x, k / 2.0, 0.5);
}

print(cdf);

print(cdf(2.0,	3.0)); # ~0.428
print(cdf(1.0,	0.5)); # ~0.846
print(cdf(-1.0,	4.0)); # 0.0
print(cdf(2.0,	-1.0)); # NA