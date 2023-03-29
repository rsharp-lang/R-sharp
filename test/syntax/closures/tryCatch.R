# catch error
const catch = try(ex -> stop(123)) {
	print([ex]::error);
	print([ex]::stackframe);
	
	# returns a default value
	# if an error has been catched
	23333;
}

print(catch);

# get try-error
const tryErr = try(stop(123));


print("get try catch error:");

print(tryErr);

print(111222333);