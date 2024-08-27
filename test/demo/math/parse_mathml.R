require(symbolic);

print(symbolic::parse.mathml(readText(file.path(@dir, "lambda.xml"))));