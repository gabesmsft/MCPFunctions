using Google.Protobuf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace FoodMCPFunctions;

public class MCPFunctions
{
    private readonly ILogger<MCPFunctions> _logger;

    public MCPFunctions(ILogger<MCPFunctions> logger)
    {
        _logger = logger;
    }

    [Function(nameof(Recommend_Food))]
    public string Recommend_Food(
        [McpToolTrigger("food_recommendation", "Food recommendation")]
        ToolInvocationContext context,
        [McpToolProperty("taste", "Choose a taste, such as sweet or savory.", isRequired: true)]
        string taste
    )
    {
        if (taste == "savory")
        {
            return "grits with butter";
        }

        if (taste == "sweet")
        {
            return "maple donuts";
        }

        return "corn fritters";
    }

    [Function(nameof(Recommend_Drink))]
    public string Recommend_Drink(
    [McpToolTrigger("drink_recommendation", "Drink recommendation")]
    [McpMetadata(ToolMetadata)]
    ToolInvocationContext context,
    [McpToolProperty("texture", "Choose a texture, such as fizzy or smooth.", isRequired: true)]
    string texture
    )
    {
        string drinkRecommendation;
        if (texture == "smooth")
        {
            drinkRecommendation = "water";
        }

        else if (texture == "fizzy")
        {
            drinkRecommendation = "marmalade soda";
        }

        else
        {
            drinkRecommendation = "coconut milk";
        }

        return drinkRecommendation;
    }

    // Optional resource metadata
    private const string ResourceMetadata = """
    {
        "ui": {
            "prefersBorder": true
        }
    }
    """;

    private const string ToolMetadata = """
        {
            "ui": {
                "resourceUri": "ui://drink/index.html"
            }
        }
        """;

    [Function(nameof(GetDrinkWidget))]
    public string GetDrinkWidget(
        [McpResourceTrigger(
        "ui://drink/index.html",
        "Drink Widget",
        MimeType = "text/html;profile=mcp-app",
        Description = "Interactive drink recommendation display for MCP Apps")]
    [McpMetadata(ResourceMetadata)]
        ResourceInvocationContext context)
    {
        var file = Path.Combine(AppContext.BaseDirectory, "index.html");
        return File.ReadAllText(file);
    }
}