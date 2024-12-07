# Screenshot Tool (F11)

A lightweight screenshot tool that captures images with the F11 key. It runs in the system tray, offering easy access and automatic upload options, along with notifications and ESC key support to exit screenshot mode.

## Build Instructions

Follow the steps below to build the project locally:

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (at least version 6.0)
- A code editor (e.g., [Visual Studio](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/))

### Steps to Build

1. **Clone the repository**:

   ```bash
   git clone https://github.com/adriantandara/screenshot-tool.git
   cd screenshot-tool
   ```

2. **Restore dependencies**:

   ```bash
   dotnet restore
   ```

3. **Build the project**:

   ```bash
   dotnet build
   ```

4. **Run the application**:
   ```bash
   dotnet run
   ```

### Publishing the Application

If you want to create a publishable version of the tool, use the following command:

```bash
dotnet publish -c Release -r win-x64 --output ./release
```
