

for(x in zip_tuple(x = [42,42,34,2,43], y = [2572,489,273,49,234], flag = "yes")) {
str(x);
}


print(zip_tuple(x = [42,42,34,2,43], y = [2572,489,273,49,234], flag = "yes", zip = [x,y,flag] -> `${flag} = ${x/y}`));