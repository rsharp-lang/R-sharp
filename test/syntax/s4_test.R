setClass("test_opts", slots = list(
    aaa = "character"
));

let obj = new("test_opts", aaa = "123");

print("before:");
print(obj@aaa);

obj@aaa = "223";

print("after:");
print(obj@aaa);

print("test missing error:");
print(obj@missing_error);