#[no_mangle]
pub extern fn demo_rust_func(x: i32) -> i32 {
    println!("Hello from Rust");
    return 42 + x;
}
