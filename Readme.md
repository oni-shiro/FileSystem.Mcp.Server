# FileSystem MCP Server

![.NET](https://img.shields.io/badge/.NET-8%2B-blue)
![Protocol](https://img.shields.io/badge/Protocol-MCP-green)
![License](https://img.shields.io/badge/license-MIT-purple)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux-lightgrey)

A **Model Context Protocol (MCP) server written in C#** that exposes **filesystem operations as AI-accessible tools**.

This project demonstrates how to build a **fully functional MCP server** capable of handling tool discovery, execution, and batch workflows.

It is designed for **AI agents, MCP clients, and developer automation systems**.

---

# Overview

The **Model Context Protocol (MCP)** allows AI systems to safely interact with external tools using a standardized interface.

This server exposes **filesystem capabilities** to MCP clients such as:

* VSCode MCP
* Claude Desktop
* Custom MCP clients
* AI agents

The server translates **JSON-RPC MCP requests** into **filesystem operations**.

---

# Architecture

```
                    MCP Client
             (VSCode / Claude / CLI)
                         │
                         │ JSON-RPC
                         ▼
                FileSystem MCP Server
                         │
                         │ Tool Dispatcher
                         ▼
                 Filesystem Tool Layer
                         │
                         ▼
                    Local Filesystem
```

Current Transport:

```
STDIN / STDOUT
```

Planned Transport:

```
HTTP
```

---

# Features

## MCP Protocol Implementation

This server implements the core MCP lifecycle methods:

| Method       | Description                                     |
| ------------ | ----------------------------------------------- |
| `initialize` | Establish connection and negotiate capabilities |
| `tools/list` | Discover available tools                        |
| `tools/call` | Execute tool operations                         |

---

## Filesystem Tools

The server exposes filesystem operations as MCP tools.

Example capabilities:

* Read files
* Write files
* List directories
* Batch filesystem operations

These tools allow AI systems to interact with the local filesystem in a structured and controlled way.

---

## Batch Processing

The server supports executing **multiple tool operations in a single request**.

Example workflow:

```
Create file
Write content
Read file
Delete file
```

All within **one MCP call**.

---

## Dependency Graph Execution

Batch operations can depend on previous operations.

Example dependency chain:

```
Operation A
   │
   ▼
Operation B
   │
   ▼
Operation C
```

The server builds a **dependency graph** and executes operations in the correct order.

---

## Cycle Detection

The batch processor prevents invalid dependency graphs.

Example invalid dependency:

```
A -> B -> C -> A
```

If a cycle is detected, the request is rejected.

---

## STDIO Transport

The server currently communicates using **STDIO transport**.

```
Client
   │
stdin/stdout JSON-RPC
   ▼
MCP Server
```

Advantages of STDIO transport:

* Fast
* Secure
* Process isolated
* Simple integration

Compatible with:

* VSCode MCP
* Claude Desktop
* Custom MCP clients

---

# Installation

Clone the repository:

```bash
git clone https://github.com/oni-shiro/FileSystem.Mcp.Server.git
```

Build the project:

```bash
dotnet build
```

Publish a standalone executable:

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

Output binary:

```
FileSystem.Mcp.Server.exe
```

---

# Running the Server

Run the executable:

```bash
FileSystem.Mcp.Server.exe
```

The server will start listening for **JSON-RPC requests on stdin**.

---

# MCP Request Examples

## Initialize

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "initialize"
}
```

---

## List Available Tools

```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "method": "tools/list"
}
```

---

## Call a Tool

Example file read request:

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

---

# Batch Execution Example

Example batch request:

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
          "id": "1",
          "tool": "write_file",
          "args": {
            "path": "file.txt",
            "content": "Hello MCP"
          }
        },
        {
          "id": "2",
          "tool": "read_file",
          "dependsOnOperationId": "1",
          "args": {
            "path": "file.txt"
          }
        }
      ]
    }
  }
}
```

Execution order:

```
write_file
    │
    ▼
read_file
```

---

# VSCode MCP Integration

Add the server to your MCP configuration.

Example:

`.vscode/mcp.json`

```json
{
  "servers": {
    "filesystem": {
      "command": "path/to/FileSystem.Mcp.Server.exe"
    }
  }
}
```

VSCode will automatically launch the server and communicate via STDIO.

---

# Testing From CLI

You can manually send requests using the console.

Example:

```powershell
'{"jsonrpc":"2.0","id":1,"method":"initialize"}' | .\FileSystem.Mcp.Server.exe
```

---

# Project Structure

```
FileSystem.Mcp.Server
│
├── Tools
│   ├── ReadFileTool
│   ├── WriteFileTool
│   └── ListDirectoryTool
│
├── BatchProcessing
│   ├── BatchProcessor
│   └── DependencyResolver
│
├── MCP Protocol
│   ├── RequestHandlers
│   └── ResponseModels
│
└── Transport
    └── StdioTransport
```

---

# Design Goals

This project focuses on:

* Clean architecture
* Protocol correctness
* Extensibility
* Safe filesystem interaction
* MCP compatibility

---

# Roadmap

Planned improvements:

## HTTP Transport

Allow remote MCP clients to connect via HTTP.

```
client -> HTTP -> MCP server
```

---

## Parallel Batch Execution

Support DAG-based execution where independent operations run in parallel.

---

## Streaming Tool Responses

Enable streaming tools for operations such as:

* tail file
* log streaming
* directory watching

---

## Security Sandbox

Restrict filesystem access to configured directories.

---

## Observability

Add logging, metrics, and tracing.

---

# Contributing

Contributions are welcome.

Steps:

1. Fork the repository
2. Create a feature branch
3. Submit a pull request

---

# License

MIT License

---

# About MCP

The **Model Context Protocol (MCP)** allows AI systems to interact with external tools through a standardized protocol.

It enables AI assistants to **discover, call, and orchestrate tools dynamically**.

Learn more:

https://modelcontextprotocol.io

---

# Author

Readme generated by AI. Need to proof read and edit.