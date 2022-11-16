wrapper = function(aa) {
	return(function() {
		aa = aa + 1;
		aa + 5;
	});
}

aa = 6;

i = wrapper(aa);

# 12
print(i());
# 13
print(i());
# 14
print(i());

# should be 6;
print(aa);