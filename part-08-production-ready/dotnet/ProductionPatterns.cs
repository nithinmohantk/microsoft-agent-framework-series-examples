using Azure.Identity;
using System.Threading.RateLimiting;

namespace MAF.Part08.Production;

public class ProductionPatterns
{
    public void IdentitySetup()
    {
        // Secure Identity Configuration
        var credential = new DefaultAzureCredential(
            new DefaultAzureCredentialOptions 
            { 
                ManagedIdentityClientId = "user-assigned-mi-client-id",
                ExcludeVisualStudioCredential = true 
            });
            
        // pass to client
    }
    
    public async Task RateLimitingDemo()
    {
        // Token Bucket Limiter
        var limiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = 100,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 10,
            ReplenishmentPeriod = TimeSpan.FromSeconds(1),
            TokensPerPeriod = 10,
            AutoReplenishment = true
        });

        using var lease = await limiter.AcquireAsync(1);
        if (lease.IsAcquired)
        {
            // Call Agent
        }
        else
        {
            // Handle throttling
        }
    }
}
