#[no_mangle]
pub extern "C" fn demo_rust_func(x: i32) -> i32 {
    println!("Hello from Rust");
    return 42 + x;
}

#[no_mangle]
pub extern "C" fn rust_string_test(str: &str) {
    println!("Hello, {}!", str);
}
