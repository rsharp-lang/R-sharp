pub fn add(left: usize, right: usize) -> usize {
    left + right
}


#[no_mangle]
pub extern "C" add(left: *const usize, right: *const usize) -> *mut usize {

} 


#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn it_works() {
        let result = add(2, 2);
        assert_eq!(result, 4);
    }
}
