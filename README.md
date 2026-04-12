## Build and Run

### Analysis Engine (Rust)

cd ~/tatpp/analysis_engine && cargo build --release

### 

cd ~/tatpp/UI && dotnet run

alias build-all='pushd C:/users/migue/desktop/proyectos/tatpp/analysis_engine && cargo build --release && popd && pushd C:/users/migue/desktop/proyectos/tatpp/ui && dotnet run && popd'