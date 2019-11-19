let x = 8;
# The linq expression is roughly equals to the procedure code show below
# The traditional R function procedure code is very verbose when compare with the linq expression
let linq_closure as function(sequence) {
	let procedure as function(x as double) {
		let y as double = x + 6;
		
		if (x <= 5) {
			return list(skip = TRUE);
		}
		
		let z = x + 5;
		
		return list(AA = z,BB = y, x^2);
	}

	result <- lapply(sequence, procedure);
	
	# x <= 5 produce list value skip=TRUE
	# removes these skiped values
	output <- list();
	
	for(attrName in names(result)) {
		if (!is.null(result[[attrName]]$skip) && result[[attrName]]$skip) {
			# skip!, do nothing 
		} else {
			output[[attrName]] = result[[attrName]];
		}
	}
	
	output;
}

zzz <- linq_closure(list(skip = x + 5, A =5,B =1, C=2,D =3,E =4));
print(zzz);