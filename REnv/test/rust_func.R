dyn.load("delaunay.dll");

a = .Call("demo_rust_func", "delaunay", x = i32(66));

print(a);