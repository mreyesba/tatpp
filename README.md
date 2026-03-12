## Build and Run

### Analysis Engine (Rust)

cd /path/to/rust_engine && cargo build --release

### 

cd /path/to/csharp_ui && dotnet run

alias build-all='pushd C:/users/migue/desktop/proyectos/tatpp/analysis_engine && cargo build --release && popd && pushd C:/users/migue/desktop/proyectos/tatpp/ui && dotnet run && popd'