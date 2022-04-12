# catch error
const catch = try(ex -> stop(123)) {
	print(ex);
	
	# returns a default value
	# if an error has been catched
	23333;
}

print(catch);

# get try-error
const tryErr = try(stop(123));

print(tryErr);