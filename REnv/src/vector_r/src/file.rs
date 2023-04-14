use std::fs;
use std::fs::OpenOptions;
use std::io::Write;
use std::path::Path;
use std::result::Result;

pub enum FileModes {
    OpenOrCreate,
    Append,
    Truncate,
}

pub fn Open(path: *const char, mode: FileModes) {
    match mode {
        FileModes::OpenOrCreate => {
            if !FileExists(path) {
                // create new
                return std::fs::File::create(path).expect("create file error!");
            } else {
                // open
                return OpenOptions::new().open(path).expect("can not open file!");
            }
        }
        FileModes::Append => {
            if !FileExists(path) {
                return Err("file missing!");
            } else {
                return OpenOptions::new()
                    .append(true)
                    .open(path)
                    .expect("can not open file!");
            }
        }
        FileModes::Truncate => {
            if !FileExists(path) {
                // just create new
                return std::fs::File::create(path).expect("create file error!");
            } else {
                return OpenOptions::new()
                    .write(true)
                    .truncate(true)
                    .open(path)
                    .expect("open file error!");
            }
        }
        _ => return Err("Unknow options!"),
    }
}

pub fn FileExists(path: *const char) -> bool {
    return Path::new(path).exists();
}
