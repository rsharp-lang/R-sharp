data = data.frame(

	a = [2,2,3,4,5],
	b = [TRUE,TRUE,TRUE,FALSE,FALSE],
	v = ["eee","3333","dddd","ssss","qqq"]
	
);

print(data);

i=1;

for([a,b,v] in data) {
	print(i);
	print(a);
	print(b);
	print(v);
	
	i=i+1;
}

for(a in (x -> stop(1))(1)) {

print("errrrrrrr");

}
