## MCP extension for Azure Functions - super-simple example

Note: This code and these instructions are provided as-is, and are not intended as a production example or a best-practices guide. Use at your own discretion.

This .NET Function App project uses the [MCP extension for Azure Functions](https://learn.microsoft.com/azure/azure-functions/functions-bindings-mcp).
This Function App consists of two MCP tools via the extension's [tool trigger](https://learn.microsoft.com/azure/azure-functions/functions-bindings-mcp-tool-trigger):
- food_recommendation: recommends a food based on taste (e.g. "sweet" or "savory", or defaults to corn fritters if no taste preference is provided.
- drink_recommendation: recommends a drink based on texture (e.g. "smooth" or "fizzy", or defaults to coconut milk if no texture is provided.

This Function App also includes an [MCP resource trigger](https://learn.microsoft.com/azure/azure-functions/functions-bindings-mcp-resource-trigger), which renders the an html page when the drink_recommendation tool is called.
> Note: The rendered resource (page) is just static content with no dynamic logic. In a real-world scenario, you might implement logic in your code, such as JavaScript, to dynamically render a resource. The purpose of this demo app is just to demonstrate the MCP resource trigger itself.


## Deploy the ARM template

[![Deploy to Azure](https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/1-CONTRIBUTION-GUIDE/images/deploytoazure.svg?sanitize=true)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fgabesmsft%2FMCPFunctions%2Fmaster%2Fdeploy%2Fazuredeploy.json)

This template deploys a Function App in Flex Consumption plan, and its resource dependencies, including a storage account and App Insights instance.

The template enables the system-assigned managed identity on the Function App, and assigns it the following roles on the storage account:
- **Storage Blob Data Owner**: used for functions host management and reading the application contents 
- **Storage Queue Data Reader** and **Storage Queue Data Message Processor**: used for the MCP extension


After the deployment completes, note down the Function App resource name.


## Deploy the code to the Function App

1. Download and extract this repository.
2. Open a command prompt, and change to the directory that contains the app.zip.
3. Log in to az cli (via az login, but you may need to run additional parameters or commands, depending on your organization's setup).
4. In the following command, replace YourFunctionAppName and ResourceGroupName, and then run the command :

```
az functionapp deployment source config-zip --src app.zip -n YourFunctionAppName -g ResourceGroupName
```

## Get the MCP endpoint and key

 1. In the Azure portal, get the Function App URL, and then construct the MCP server URL as https://YourFunctionAppHostNamePrefix.azurewebsites.net/runtime/webhooks/mcp , where YourFunctionAppHostNamePrefix is your hostname prefix.  You will use this URL in a later step.
 2. On the Function App, get the mcp_extension key. This can currently be found on the Functions blade. You will use this key to connect to the MCP server in a later step.


## Configure the client connection

Here are a couple examples for configuring a connection to the MCP server on a client. This is not meant to be an exhaustive list. Also, the UIs in the examples are subject to change.

###  Azure SRE Agent

#### Configure the connection

Add an **MCP server** connector with the following settings (replace *italicized* values:

- Connection type: Streamable-HTTP
- URL: https://*YourFunctionAppHostNamePrefix*.azurewebsites.net/runtime/webhooks/mcp
- Authentication method: Custom headers
  - key: x-functions-key
  - value: *your mcp_extension key*
- Tools: *after successfully testing the connection, select all available tools*

 ### Visual Studio Code

 #### Configure the connection

 1. In Visual Studio Code, open a folder. You will create an mcp.json file in this folder.
 2. Click View | Command Palette.
 3. Enter **MCP: Add Server**
 4. Enter **HTTP (HTTP or Server-Sent Events)**
 5. Enter the MCP server URL you constructed earlier.
 6. Accept the default for the local MCP server connection name, or give it an arbitrary name of your choice.
 7. When asked where to install the local server, select **Workspace**.
 8. When prompted, click **Trust** to trust the connection.
 9. If VS Code prompts you to authenticate to Microsoft, click Cancel.

    This should create an mcp.json in the VS Code folder.

 10. Update the mcp.json so that it resembles the following (replace my-mcp-server-hashValue and YourFunctionAppHostNamePrefix):

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

 11. Under **servers** in the mcp.json in VS Code, click Start if the server isn't Running, or click Restart if it shows to be in Error state.
 12. When prompted for the mcp key, enter the mcp_extension key you obtained earlier.

  > Note: The prompt is not a dialog box, and is easy to miss if you are not actively looking for it. Th prompt would be in the top search & command area of VS Code, and would prompt with text such as "Azure Functions MCP Extension System Key". If you've set up the mcp.json configuration multiple times, a previous key might be cached and you might not get prompted. In this case, you can try steps such as running the MCP: Reset Cached Tools command, restarting VS Code, and/or changing the name of the inputs in the mcp.json from functions-mcp-extension-system-key to something else (e.g. functions-mcp-extension-system-key-2). This is not a definitive guide for troubleshooting issues with not getting prompted for the key, so consult with the VS Code community if you are still stuck with getting a prompt.

 13. In the Output pane, select **MCP: *local connection name*** and then check the Output pane for connection activity.
If the connection is successful, the output should show entries that resemble the following:

```
<timestamp>[info] Connection state: Running
<timestamp>[info] Discovered 2 tools
```

  For the lifetime of the local MCP configuration and Function App and mcp key, you should not need to complete all the steps again, beyond clicking Start or Restart in the MCP server connection.

#### Test the MCP tools
1. In Visual Studio Code, select **View | Chat**.
2. Verify that **Agent** is selected in the Chat pane.
3. Ask the chat a question such as **What's a good food that is savory?**. It should find the Function App's food_recommendation tool and respond with the tool's response of "grits with butter" for savory, along with some additional information about grits with butter that the VS Code chat retrieves from its langauge model.
   Note: If prompted to Approve, select Approve or click the drop-down and select the option to auto-approve.
4. Ask the chat a question such as **What's a good food that is bitter?**. It should respond with the food_recommendation's default response of "corn fritters" when a taste isn't provided or matched.
5. Ask other questions that relate to the food_recommendation or drink_recommendation tool.

If asking about a drink recommendation, the resource trigger should return a static list of drink recipes, starting with a header of "Drink recommendations webpage", which is the rendered index.html page specified by the resource trigger. Additionally, the resource trigger should trigger the mcp tool trigger, which returns its drink recommendation. 

