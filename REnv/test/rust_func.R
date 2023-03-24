dyn.load("E:\GCModeller\src\R-sharp\App\net6.0\delaunay.dll");

a = .Call("demo_rust_func", "delaunay", x = i32(66));

print(a);