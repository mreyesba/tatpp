use std::ffi::CStr;
use std::fs::File;
use libc::c_char;
use memmap2::Mmap;

#[unsafe(no_mangle)]
pub extern "C" fn analyze_massive_file(path_ptr: *const c_char) -> i64 {
    if path_ptr.is_null() { return -1; }

    // 1. Convert C String from .NET to Rust String
    let c_str = unsafe { CStr::from_ptr(path_ptr) };
    let path = match c_str.to_str() {
        Ok(s) => s,
        Err(_) => return -2,
    };

    // 2. Open and Memory Map the file
    let file = match File::open(path) {
        Ok(f) => f,
        Err(_) => return -3,
    };

    let mmap = unsafe { 
        match Mmap::map(&file) {
            Ok(m) => m,
            Err(_) => return -4,
        }
    };

    // 3. Example Analysis: Count every byte (this won't crash on 10GB!)
    // For 10GB, the OS will swap chunks in and out of RAM automatically.
    let count = mmap.len() as i64;

    println!("Rust Engine: Successfully mapped {} bytes", count);
    
    count
}
