## MCP extension for Azure Functions - super-simple example

Note: This code and these instructions are provided as-is, and are not intended as a production example or a best-practices guide. Use at your own discretion.

This .NET Function App project uses the [MCP trigger for Azure Functions](https://learn.microsoft.com/azure/azure-functions/functions-bindings-mcp-trigger).
This Function App consists of two MCP tools:
- food_recommendation: recommends a food based on taste (e.g. "sweet" or "savory", or defaults to corn fritters if no taste preference is provided.
- drink_recommendation: recommends a drink based on texture (e.g. "smooth" or "fizzy", or defaults to coconut milk if no texture is provided.
 
 The options provided in code are very limited, but demonstrate the functionality of the MCP tooling at a simple level.
 
 ## Function App
 1. Deploy the project to a Function App.
 2. If the Function App is using a managed identity to connect to the storage account used by the AzureWebJobsStorage setting, ensure that the identity has the necessary queue roles on the storage account as described [here](storage account).
 3. Get the Function App host name, and then construct the MCP server URL as https://YourFunctionAppHostNamePrefix.azurewebsites.net/runtime/webhooks/mcp , where YourFunctionAppHostNamePrefix is your hostname prefix.  You will use this URL in a later step.
 4. On the Function App, get the mcp_extension key. You will use this to connect to the MCP server in a later step.
 
 ## Visual Studio Code
 ### Set up the MCP server connection
 1. In Visual Studio Code, open a new / empty folder. The folder you choose doesn't matter much, but you will create an mcp.json file in this folder.
 2. Click View | Command Palette.
 3. Enter **MCP: Add Server**
 4. Enter **HTTP (HTTP or Server-Sent Events)**
 5. Enter the MCP server URL you constructed earlier.
 6. Accept the default for the local MCP server connection name, or give it an arbitrary name of your choice.
 7. When asked where to install the local server, select **Workspace**.
 8. When prompted, click **Trust** to trust the connection.
 9. If VS Code prompts you to authenticate via Oauth and provide a client id / application id, [configure Authentication](https://learn.microsoft.com/azure/app-service/configure-authentication-provider-aad) on the Function App, and then provide the client id to VS Code.
    Note: This is not a Function App requirement for using MCP authentication.

This should create an mcp.json in the VS Code folder.

### Finish configuration of the mcp.json

Update the mcp.json so that it resembles the following (replace my-mcp-server-hashValue and YourFunctionAppHostNamePrefix):

```
{
	"servers": {
		"my-mcp-server-hashValue": {
			"url": "https://YourFunctionAppHostNamePrefix.azurewebsites.net/runtime/webhooks/mcp",
			"type": "http",
            "headers": {
                "x-functions-key": "${input:functions-mcp-extension-system-key}"
            }
		}
	},
    "inputs": [
        {
            "type": "promptString",
            "id": "functions-mcp-extension-system-key",
            "description": "Azure Functions MCP Extension System Key",
            "password": true
        }
    ]
}
```

### Verify that the connection works
1. Under **servers** in the mcp.json in VS Code, click Start if the server isn't Running.
2. When prompted for the mcp key, enter the mcp_extension key you obtained earlier.
3. In the Output pane, select **MCP: *local connection name*** and then check the Output pane for connection activity.
If the connection is successful, the output should show entries that resemble the following:

```
<timestamp>[info] Connection state: Running
<timestamp>[info] Discovered 2 tools
```

### Test the MCP Functions
1. In Visual Studio Code, select **View | Chat**.
2. Verify that **Agent** is selected in the Chat pane.
3. Ask the chat a question such as **What's a good food that is savory?**. It should find the Function App's food_recommendation tool and respond with the tool's response of "grits with butter" for savory, along with some additional information about grits with butter that the VS Code chat retrieves from its langauge model.
   Note: If prompted to Approve, select Approve or click the drop-down and select the option to auto-approve.
4. Ask the chat a question such as **What's a good food that is bitter?**. It should respond with the food_recommendation's default response of "corn fritters" when a taste isn't provided or matched.
5. Ask other questions that relate to the food_recommendation or drink_recommendation tool.