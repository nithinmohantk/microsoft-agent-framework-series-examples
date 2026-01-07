using System.ComponentModel;
using Microsoft.Agents.AI;

namespace MAF.Part10.Migration;

/// <summary>
/// Part 10: Plugin/Tool Migration - BEFORE (SK Pattern)
/// </summary>
public class BeforeSemanticKernelPlugins
{
    /*
    // Semantic Kernel plugins with [KernelFunction]
    public class WeatherPlugin
    {
        [KernelFunction]
        [Description("Get current weather for a city")]
        public string GetWeather(
            [Description("City name")] string city,
            [Description("Units: celsius or fahrenheit")] string units = "celsius")
        {
            return $"Weather in {city}: 72°F Sunny";
        }
    }

    // Registration
    kernel.Plugins.AddFromType<WeatherPlugin>();
    */
}

/// <summary>
/// Part 10: Tool Migration - AFTER (MAF Pattern)
/// </summary>
public static class AfterAgentFrameworkTools
{
    // Agent Framework uses [Description] attributes directly on methods
    [Description("Get current weather for a city")]
    public static string GetWeather(
        [Description("City name")] string city,
        [Description("Units: celsius or fahrenheit")] string units = "celsius")
    {
        var weatherData = new Dictionary<string, (int Temp, string Condition)>
        {
            ["new york"] = (72, "Sunny"),
            ["london"] = (58, "Cloudy"),
        };

        var data = weatherData.GetValueOrDefault(city.ToLower(), (70, "Unknown"));
        var temp = units == "fahrenheit" ? data.Temp : (data.Temp - 32) * 5 / 9;
        return $"Weather in {city}: {temp}°{(units == "fahrenheit" ? "F" : "C")}, {data.Condition}";
    }

    [Description("Get multi-day weather forecast")]
    public static string GetForecast(
        [Description("City name")] string city,
        [Description("Number of days (1-10)")] int days = 5)
    {
        return $"{days}-day forecast for {city}: Partly cloudy, highs in 70s";
    }

    // Usage - tools are passed directly to CreateAIAgent
    public static void Demo()
    {
        /*
        var agent = client.CreateAIAgent(
            name: "WeatherAgent",
            instructions: "Help with weather questions.",
            tools: new object[]
            {
                GetWeather,
                GetForecast
            });
        */
    }
}
