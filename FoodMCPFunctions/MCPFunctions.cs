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
        [McpToolProperty("taste", "string", "Choose a taste, such as sweet or savory.")]
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
    ToolInvocationContext context,
    [McpToolProperty("texture", "string", "Choose a texture, such as fizzy or smooth.")]
        string texture
)
    {
        if (texture == "smooth")
        {
            return "water";
        }

        if (texture == "fizzy")
        {
            return "marmalade soda";
        }

        return "coconut milk";
    }
}