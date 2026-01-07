using System.Text.RegularExpressions;

namespace MAF.Part08.Security;

/// <summary>
/// Part 8: Content Safety Filter for .NET
/// </summary>
public enum SafetyCategory
{
    Harmful,
    PII,
    Jailbreak,
    Profanity,
    BlockedTerm
}

public record SafetyResult(bool IsSafe, List<SafetyCategory> Violations, string Details);

public class ContentSafetyFilter
{
    private readonly bool _blockPii;
    private readonly bool _blockJailbreaks;
    private readonly HashSet<string> _blocklist;
    private readonly int _maxInputLength;

    private readonly List<(Regex Pattern, string PiiType)> _piiPatterns = new()
    {
        (new Regex(@"\b\d{3}-\d{2}-\d{4}\b"), "SSN"),
        (new Regex(@"\b\d{16}\b"), "Credit Card"),
        (new Regex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", RegexOptions.IgnoreCase), "Email"),
        (new Regex(@"\b\d{3}[-.]?\d{3}[-.]?\d{4}\b"), "Phone"),
    };

    private readonly List<Regex> _jailbreakPatterns = new()
    {
        new Regex(@"ignore (previous|all|your) instructions", RegexOptions.IgnoreCase),
        new Regex(@"pretend (you are|to be)", RegexOptions.IgnoreCase),
        new Regex(@"act as (if you are|a)", RegexOptions.IgnoreCase),
        new Regex(@"disregard (safety|guidelines)", RegexOptions.IgnoreCase),
        new Regex(@"bypass (filters|safety)", RegexOptions.IgnoreCase),
    };

    public ContentSafetyFilter(
        bool blockPii = true,
        bool blockJailbreaks = true,
        IEnumerable<string>? customBlocklist = null,
        int maxInputLength = 4000)
    {
        _blockPii = blockPii;
        _blockJailbreaks = blockJailbreaks;
        _blocklist = new HashSet<string>(customBlocklist ?? Enumerable.Empty<string>(), 
            StringComparer.OrdinalIgnoreCase);
        _maxInputLength = maxInputLength;
    }

    public SafetyResult CheckInput(string text)
    {
        var violations = new List<SafetyCategory>();
        var details = new List<string>();

        // Length check
        if (text.Length > _maxInputLength)
        {
            violations.Add(SafetyCategory.Harmful);
            details.Add($"Input exceeds max length ({text.Length} > {_maxInputLength})");
        }

        // PII check
        if (_blockPii)
        {
            foreach (var (pattern, piiType) in _piiPatterns)
            {
                if (pattern.IsMatch(text))
                {
                    violations.Add(SafetyCategory.PII);
                    details.Add($"Potential {piiType} detected");
                }
            }
        }

        // Jailbreak check
        if (_blockJailbreaks)
        {
            foreach (var pattern in _jailbreakPatterns)
            {
                if (pattern.IsMatch(text))
                {
                    violations.Add(SafetyCategory.Jailbreak);
                    details.Add("Potential jailbreak attempt detected");
                    break;
                }
            }
        }

        // Blocklist check
        foreach (var term in _blocklist)
        {
            if (text.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                violations.Add(SafetyCategory.BlockedTerm);
                details.Add("Blocked term detected");
            }
        }

        var isSafe = violations.Count == 0;
        return new SafetyResult(
            isSafe,
            violations.Distinct().ToList(),
            details.Any() ? string.Join("; ", details) : "No issues detected"
        );
    }

    public string SanitizeOutput(string text)
    {
        var result = text;
        foreach (var (pattern, piiType) in _piiPatterns)
        {
            result = pattern.Replace(result, $"[{piiType} REDACTED]");
        }
        return result;
    }
}
