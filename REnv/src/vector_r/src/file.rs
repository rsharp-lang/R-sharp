use std::fs::File;
use std::fs::OpenOptions;
use std::io::Write;
use std::path::Path;

pub enum FileModes {
    OpenOrCreate,
    Append,
    Truncate,
}

pub fn Open(path: &str, mode: FileModes) -> Result<File, &str> {
    match mode {
        FileModes::OpenOrCreate => {
            if !FileExists(path) {
                // create new
                return Ok(std::fs::File::create(path).expect("create file error!"));
            } else {
                // open
                return Ok(OpenOptions::new().open(path).expect("can not open file!"));
            }
        }
        FileModes::Append => {
            if !FileExists(path) {
                return Err("file missing!");
            } else {
                return Ok(OpenOptions::new()
                    .append(true)
                    .open(path)
                    .expect("can not open file!"));
            }
        }
        FileModes::Truncate => {
            if !FileExists(path) {
                // just create new
                return Ok(std::fs::File::create(path).expect("create file error!"));
            } else {
                return Ok(OpenOptions::new()
                    .write(true)
                    .truncate(true)
                    .open(path)
                    .expect("open file error!"));
            }
        }
        _ => return Err("Unknow options!"),
    }
}

pub fn FileExists(path: &str) -> bool {
    return Path::new(path).exists();
}

pub fn WriteLines(x: Vec<&str>, mut con: File) {
    for val in x.iter() {
        con.write_all(val.chars().as_str().as_bytes());
    }
}

pub fn WriteLine(x: &str, mut con: File) {
    con.write_all(x.as_bytes());
}
