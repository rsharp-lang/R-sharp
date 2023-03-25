dyn.load("E:\GCModeller\src\R-sharp\REnv\src\delaunay\target\release\delaunay.dll");

# a = .Call("demo_rust_func", "delaunay", x = i32(66), return = "i32");

# print(a);

.Call("rust_string_test", "delaunay", x = string("value from R# language"));

str(.Call("get_rust_str", "delaunay", str = string("AAABBBCCCDDD"), return = "string"));