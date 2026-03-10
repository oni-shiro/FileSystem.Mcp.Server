<div align="center">

# FileSystem.Mcp.Server

**A Model Context Protocol server written in C# that exposes filesystem operations as AI-accessible tools.**

[![.NET](https://img.shields.io/badge/.NET-8%2B-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![MCP Protocol](https://img.shields.io/badge/MCP-Compatible-00A86B?style=flat-square)](https://modelcontextprotocol.io)
[![Transport](https://img.shields.io/badge/Transport-STDIO-0078D4?style=flat-square)]()
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux-lightgrey?style=flat-square)]()
[![License](https://img.shields.io/badge/License-MIT-blueviolet?style=flat-square)](LICENSE)

[Getting Started](#getting-started) · [Usage](#usage) · [Integrations](#integrations) · [Roadmap](#roadmap) · [Contributing](#contributing)

</div>

---

## What is this?

`FileSystem.Mcp.Server` is a [Model Context Protocol (MCP)](https://modelcontextprotocol.io) server built in C# that gives AI clients structured, controlled access to the local filesystem.

It translates JSON-RPC MCP requests into real filesystem operations — read files, write files, list directories — and supports chaining multiple operations together in a single batch call with dependency ordering.

**Works out of the box with:**
- [VSCode MCP](https://code.visualstudio.com/)
- [Claude Desktop](https://claude.ai/download)
- Any custom MCP client over STDIO

---

## Features

- **Full MCP lifecycle** — `initialize`, `tools/list`, `tools/call`
- **Core filesystem tools** — read, write, list directory
- **Batch execution** — run multiple operations in one MCP call
- **Dependency graph ordering** — declare operation dependencies, execute in the right sequence
- **Cycle detection** — circular dependency graphs are caught and rejected before any operations run
- **STDIO transport** — fast, secure, process-isolated communication

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download) or later
- Windows or Linux

### Build

```bash
git clone https://github.com/oni-shiro/FileSystem.Mcp.Server.git
cd FileSystem.Mcp.Server
dotnet build
```

### Publish

```bash
# Windows (self-contained)
dotnet publish -c Release -r win-x64 --self-contained

# Linux (self-contained)
dotnet publish -c Release -r linux-x64 --self-contained
```

### Run

```bash
# Windows
.\FileSystem.Mcp.Server.exe

# Linux
./FileSystem.Mcp.Server
```

The server starts and listens for JSON-RPC requests on `stdin`.

---

## Usage

### Initialize

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "initialize"
}
```

### List Tools

```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "method": "tools/list"
}
```

### Call a Tool

```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "method": "tools/call",
  "params": {
    "name": "read_file",
    "arguments": {
      "path": "example.txt"
    }
  }
}
```

### Available Tools

| Tool | Description |
|---|---|
| `read_file` | Read the contents of a file |
| `write_file` | Write content to a file |
| `list_directory` | List files and folders in a directory |
| `batch` | Execute multiple operations in one call |

---

## Batch Execution

The `batch` tool lets you chain operations in a single request. Use `dependsOnOperationId` to control execution order — the server builds a dependency graph and runs operations in the correct sequence.

```json
{
  "jsonrpc": "2.0",
  "id": 10,
  "method": "tools/call",
  "params": {
    "name": "batch",
    "arguments": {
      "operations": [
        {
          "id": "op-1",
          "tool": "write_file",
          "args": {
            "path": "hello.txt",
            "content": "Hello, MCP!"
          }
        },
        {
          "id": "op-2",
          "tool": "read_file",
          "dependsOnOperationId": "op-1",
          "args": {
            "path": "hello.txt"
          }
        }
      ]
    }
  }
}
```

**Execution order:** `write_file` → `read_file`

> ⚠️ **Cycle detection:** If operations form a circular dependency (e.g. A → B → C → A), the entire request is rejected before any operations execute.

---

## Integrations

### Connecting to Claude Desktop

You can connect this server to [Claude Desktop](https://claude.ai/download) so Claude can read, write, and manage files on your machine through natural conversation.

**Step 1 — Build the server**

First, publish a self-contained binary (see [Getting Started](#getting-started)).

**Step 2 — Open the Claude Desktop config file**

In Claude Desktop, go to **Settings → Developer → Edit Config**. This opens `claude_desktop_config.json` in your file editor.

Config file locations:
- **Windows:** `%APPDATA%\Claude\claude_desktop_config.json`
- **macOS:** `~/Library/Application Support/Claude/claude_desktop_config.json`

**Step 3 — Add the server**

Add the following to your config, replacing the path with the actual location of your published binary:

```json
{
  "mcpServers": {
    "filesystem": {
      "command": "C:\\path\\to\\FileSystem.Mcp.Server.exe"
    }
  }
}
```

On Linux/macOS:

```json
{
  "mcpServers": {
    "filesystem": {
      "command": "/path/to/FileSystem.Mcp.Server"
    }
  }
}
```

**Step 4 — Restart Claude Desktop**

Fully quit and relaunch Claude Desktop. On restart it will automatically launch the MCP server.

**Step 5 — Verify the connection**

Click the **`+`** button at the bottom of the chat box, then select **Connectors** to see the connected server and its available tools. You can also check **Settings → Developer** for connection status and logs.

Once connected, you can ask Claude things like:
- *"List the files in my Downloads folder"*
- *"Read the contents of config.json"*
- *"Write a new file called notes.txt with the following content…"*

> **Troubleshooting:** If the server doesn't appear, double-check the file path in your config is absolute (not relative), and that the binary has execute permissions on Linux/macOS (`chmod +x FileSystem.Mcp.Server`). Logs are available at `~/Library/Logs/Claude/` (macOS) or `%APPDATA%\Claude\logs\` (Windows).

---

### VSCode

Add to `.vscode/mcp.json` in your workspace:

```json
{
  "servers": {
    "filesystem": {
      "command": "path/to/FileSystem.Mcp.Server"
    }
  }
}
```

VSCode will launch the server automatically and communicate over STDIO.

### CLI (manual testing)

```powershell
# Windows PowerShell
'{"jsonrpc":"2.0","id":1,"method":"initialize"}' | .\FileSystem.Mcp.Server.exe
```

```bash
# Linux / macOS
echo '{"jsonrpc":"2.0","id":1,"method":"initialize"}' | ./FileSystem.Mcp.Server
```

---

## Architecture

```
MCP Client
(VSCode / Claude Desktop / CLI)
          │
          │  JSON-RPC (STDIO)
          ▼
┌─────────────────────────────┐
│      FileSystem MCP Server  │
│                             │
│  ┌──────────────────────┐   │
│  │   Request Handlers   │   │
│  └──────────┬───────────┘   │
│             │               │
│  ┌──────────▼───────────┐   │
│  │   Tool Dispatcher    │   │
│  └──────────┬───────────┘   │
│             │               │
│  ┌──────────▼───────────┐   │
│  │   Batch Processor    │   │
│  │   + Dep. Resolver    │   │
│  └──────────┬───────────┘   │
└─────────────┼───────────────┘
              │
              ▼
       Local Filesystem
```

**Current transport:** STDIO · **Planned:** HTTP

---

## Project Structure

```
FileSystem.Mcp.Server/
├── Tools/
│   ├── ReadFileTool.cs
│   ├── WriteFileTool.cs
│   └── ListDirectoryTool.cs
├── BatchProcessing/
│   ├── BatchProcessor.cs
│   └── DependencyResolver.cs
├── Protocol/
│   ├── RequestHandlers.cs
│   └── ResponseModels.cs
└── Transport/
    └── StdioTransport.cs
```

---

## Roadmap

- [ ] **HTTP Transport** — Allow remote MCP clients to connect over HTTP
- [ ] **Parallel Batch Execution** — Run independent operations concurrently using DAG scheduling
- [ ] **Streaming Tool Responses** — Support `tail`, log streaming, and directory watching
- [ ] **Security Sandbox** — Restrict filesystem access to configured directories only
- [ ] **Observability** — Structured logging, metrics, and tracing

---

## Contributing

Contributions are welcome. Please open an issue first for significant changes so we can align before you build.

1. Fork the repo
2. Create a feature branch: `git checkout -b feat/my-feature`
3. Commit your changes: `git commit -m "feat: add my feature"`
4. Push and open a pull request

---

## License

Distributed under the [MIT License](LICENSE).

---

<div align="center">
  <sub>Built with C# · Powered by <a href="https://modelcontextprotocol.io">Model Context Protocol</a></sub>
</div>