using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MAF.Part04.Tools;

/// <summary>
/// Part 4: Advanced Tools in .NET
/// </summary>
public class AdvancedTools
{
    private static readonly HttpClient _httpClient = new();
    
    // Pattern 1: Simple tool with descriptions
    [Description("Get current weather for a location")]
    public static string GetWeather(
        [Description("City name")] string city,
        [Description("Temperature unit: celsius or fahrenheit")] string units = "celsius")
    {
        var weatherData = new Dictionary<string, (int Temp, string Condition)>
        {
            ["new york"] = (22, "Sunny"),
            ["london"] = (15, "Cloudy"),
            ["tokyo"] = (28, "Humid")
        };
        
        var data = weatherData.GetValueOrDefault(city.ToLower(), (20, "Unknown"));
        var temp = units == "fahrenheit" ? (data.Temp * 9 / 5) + 32 : data.Temp;
        var unit = units == "fahrenheit" ? "F" : "C";
        
        return $"Weather in {city}: {temp}°{unit}, {data.Condition}";
    }
    
    // Pattern 2: Async tool for external calls
    [Description("Fetch data from an external REST API")]
    public static async Task<string> FetchApiDataAsync(
        [Description("Full API endpoint URL")] string endpoint)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsStringAsync();
            return $"API returned status {(int)response.StatusCode}";
        }
        catch (HttpRequestException ex)
        {
            return $"API error: {ex.Message}";
        }
    }
    
    // Pattern 3: Complex operations with validation
    [Description("Create a support ticket in the system")]
    public static string CreateTicket(
        [Description("Customer email address")] string email,
        [Description("Issue description")] string description,
        [Description("Priority: Low, Medium, High, Critical")] string priority = "Medium")
    {
        if (!email.Contains("@"))
            return "Error: Invalid email format.";
        
        if (string.IsNullOrWhiteSpace(description))
            return "Error: Description cannot be empty.";
        
        var ticketId = $"TKT-{DateTime.Now:yyyyMMddHHmmss}";
        return $"Created ticket {ticketId} for {email} with priority {priority}";
    }
    
    // Pattern 4: Search with pagination
    [Description("Search documents in the repository")]
    public static string SearchDocuments(
        [Description("Search query")] string query,
        [Description("Maximum results (1-20)")] int limit = 10)
    {
        limit = Math.Clamp(limit, 1, 20);
        var results = new List<string>();
        
        for (int i = 1; i <= Math.Min(limit, 3); i++)
        {
            results.Add($"{i}. Document about {query} - /docs/{i}");
        }
        
        return $"Found {results.Count} results:\n" + string.Join("\n", results);
    }
}
