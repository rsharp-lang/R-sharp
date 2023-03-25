use libc::c_char;
use std::ffi::CStr;
use std::str;

#[no_mangle]
pub extern "C" fn demo_rust_func(x: i32) -> i32 {
    println!("Hello from Rust");
    return 42 + x;
}

#[no_mangle]
pub extern "C" fn rust_string_test(str: *const c_char) {
    println!("Hello, {}!", mkstr(str));
}

/**
 * cast C string to rust string
*/
fn mkstr(s: *const c_char) -> String {
    let c_str: &CStr = unsafe { CStr::from_ptr(s) };
    let r_str = c_str.to_str().unwrap();
    String::from(r_str)
}
