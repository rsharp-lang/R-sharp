using a as file('./test.txt') {
	# call System.IDispose
	# operations with variable a
	print((a :> as.object)$Name);
}

# call a$Dispose()
# and then delete a from the current environment
let a as string = 'abc: ' & $;

print(a);