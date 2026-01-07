using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Microsoft.Extensions.Logging;

namespace MAF.Part08.Resilience;

/// <summary>
/// Part 8: Resilient Agent Wrapper with Circuit Breaker for .NET
/// </summary>
public class ResilientAgent
{
    private readonly object _agent;
    private readonly ILogger _logger;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy _circuitBreaker;
    private readonly AsyncTimeoutPolicy _timeoutPolicy;
    private readonly string _fallbackResponse;

    public ResilientAgent(
        object agent,
        ILogger logger,
        int maxRetries = 3,
        int circuitBreakerThreshold = 5,
        int circuitBreakerDuration = 60,
        int timeoutSeconds = 60,
        string? fallbackResponse = null)
    {
        _agent = agent;
        _logger = logger;
        _fallbackResponse = fallbackResponse 
            ?? "I'm experiencing difficulties. Please try again later.";

        // Timeout policy
        _timeoutPolicy = Policy.TimeoutAsync(
            TimeSpan.FromSeconds(timeoutSeconds),
            TimeoutStrategy.Optimistic);

        // Retry policy with exponential backoff
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                maxRetries,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Retry {RetryCount} after {Delay}s due to: {Message}",
                        retryCount, timeSpan.TotalSeconds, exception.Message);
                });

        // Circuit breaker policy
        _circuitBreaker = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: circuitBreakerThreshold,
                durationOfBreak: TimeSpan.FromSeconds(circuitBreakerDuration),
                onBreak: (exception, duration) =>
                {
                    _logger.LogError(
                        "Circuit OPENED for {Duration}s due to: {Message}",
                        duration.TotalSeconds, exception.Message);
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit CLOSED - resuming normal operation");
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation("Circuit HALF-OPEN - testing...");
                });
    }

    public async Task<string> RunAsync(string message, object? thread = null)
    {
        try
        {
            // Combine policies: timeout -> retry -> circuit breaker
            var combinedPolicy = Policy.WrapAsync(_timeoutPolicy, _retryPolicy, _circuitBreaker);

            var result = await combinedPolicy.ExecuteAsync(async () =>
            {
                // Use reflection to call the agent's RunAsync method
                var runMethod = _agent.GetType().GetMethod("RunAsync");
                if (runMethod == null)
                    throw new InvalidOperationException("Agent does not have RunAsync method");

                dynamic task = runMethod.Invoke(_agent, new[] { message, thread })!;
                return await task;
            });

            return result?.ToString() ?? string.Empty;
        }
        catch (BrokenCircuitException)
        {
            _logger.LogWarning("Circuit breaker is open - returning fallback");
            return _fallbackResponse;
        }
        catch (TimeoutRejectedException)
        {
            _logger.LogWarning("Request timed out - returning fallback");
            return _fallbackResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "All retries exhausted - returning fallback");
            return _fallbackResponse;
        }
    }
}
