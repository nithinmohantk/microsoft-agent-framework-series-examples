using Microsoft.Agents.AI;
    
public class HumanInLoopWorkflow
{
    public enum ApprovalStatus { Pending, Approved, Rejected }
    
    public record ApprovalRequest(string Id, string Action, object Data, ApprovalStatus Status = ApprovalStatus.Pending);
    
    private readonly Dictionary<string, ApprovalRequest> _pending = new();
    
    public async Task<string> ProcessAction(object agent, string request, object thread)
    {
        // Agent processes request
        // ... (Simulated Agent Run)
        var response = "[NEEDS_APPROVAL] Deploy to Production"; 
        
        if (response.Contains("[NEEDS_APPROVAL]"))
        {
             var reqId = Guid.NewGuid().ToString();
             _pending[reqId] = new ApprovalRequest(reqId, "Deploy", response);
             return $"Action Requires Approval. ID: {reqId}";
        }
        
        return response;
    }
    
    public void Approve(string id) 
    {
        if (_pending.ContainsKey(id))
            _pending[id] = _pending[id] with { Status = ApprovalStatus.Approved };
    }
}
