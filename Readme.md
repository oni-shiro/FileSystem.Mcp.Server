# File System MCP server in C#
This is a fully working mcp server written in C#, this is a pluggable MCP server that enables any other project or tool to use to expose file system operations to LLMs via mcp clients.

Right now it is STDIN type of server, I am currently working on adding the HTTP support to make it a remote server

## Features 🗃️
This exposes basic File system I/0 operations as well as tools to run batch operations that will save token cost. This runs on a sandbox, basically server restricts access to any file or folder outside the given root folder. If not set it will be set as the current workspace of the server.

Otherwise it can be set via params in `dotnet` `run` 
 
or more suited if ran using the self-contained exe, is to set `MCP_ROOT_DIR` in environment variables. All paths should be a child of this parent directory otherwise the execution will be cancelled.

For single file operations the requst JRPC is quite simple.

So I will give an example of the batch processing below:

Let's say there are couple of files in the root directory(it can be in any other folders inside the root, I am taking files in only root for simplicity). Say the files are named as 1.txt,2.txt and so on. You want to do some operations on them, so the expected requested Json will be:

```[
  {
    "id": "1",
    "type": "read",
    "path": "1.txt"
  },
  {
    "id": "2",
    "type": "read",
    "path": "2.txt"
  },
  {
    "id": "3",
    "type": "write",
    "path": "3.txt",
    "dependsonoperationids": [
      "2",
      "4"
    ],
    "content": "File name is changed to 3.13"
  },
  {
    "id": "4",
    "type": "CopyFile",
    "path": "2.txt",
    "targetpath":".\\tmp_child\\2_copy.txt"
  }
]
```

Here we are defining the operationId, type of operation(basically calls the single file opertions), path to the file, and r`quired params,
Like for copy we need the `targetPath`, for writing in file we need the `content`.

I would like to draw your attention to the `dependsonoperationids` param. It basically stores the dependencies for that operation to complete. Here I am using BFS to detect cyle and creat the ordered list of exectution. If any depenncy loop is detected, it will throw an error. 

[Rest of the readme will be added later, I would prefer to use ai to write it to negate manual labour💀]


## How to run it using vscode
Adding the below section in your global/workspace mcp.json should work:
```
		"filesystem-mcp-server6f2add0c": {
			"type": "stdio",
			"command": "dotnet",
			"args": [
				"run",
				"--project",
				"<absolute-path-to-project>\\FileSystem.Mcp.Server.csproj"
			],
			"env": {
				"MCP_ROOT_DIR": "<root-dir>"
			}
		}

```  