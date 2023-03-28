use libc::c_char;
use std::ffi::CStr;
use std::ffi::CString;
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

#[no_mangle]
pub extern "C" fn get_rust_str(str: *const c_char) -> *mut c_char {
    let data = mkstr(str);
    let mut rust_str = String::from("this is the string from rust, ");

    rust_str.push_str("and get value from R#:");
    rust_str.push_str(data.as_str());
    rust_str.push_str("; that's it!");

    let cstr = CString::new(rust_str).expect("memory error!");

    return cstr.into_raw();
}

/**
 * cast C string to rust string
*/
fn mkstr(s: *const c_char) -> String {
    let c_str: &CStr = unsafe { CStr::from_ptr(s) };
    let r_str = c_str.to_str().unwrap();
    String::from(r_str)
}
