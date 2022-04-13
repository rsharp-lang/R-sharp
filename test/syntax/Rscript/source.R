# when source a given script by path
# then an object list variable with special name will be push into 
# the environment
# 
# let !script = list(dir = dirname, file = filename, fullName = filepath)

print("View of the special variable for source invoke in R#");

print("directory that contains script");
print(!script$dir);
print("file name of the script");
print(!script$file);
print("full path of the script");
print(!script$fullName);