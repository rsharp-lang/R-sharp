setClass("test_opts", slots = list(
    aaa = "character"
));

let obj = new("test_opts", aaa = "123");

obj@aaa = "223";

print(obj@aaa);
print(obj@missing_error);