"""
Part 5: Human-in-the-Loop Workflow
"""
import asyncio
from dataclasses import dataclass
from typing import Optional
from enum import Enum


class ApprovalStatus(Enum):
    PENDING = "pending"
    APPROVED = "approved"
    REJECTED = "rejected"


@dataclass
class ActionRequest:
    action: str
    params: dict
    requires_approval: bool
    reason: str


# In-memory store for pending approvals (use Redis/DB in production)
pending_approvals = {}


async def check_if_approval_needed(action: str, params: dict) -> ActionRequest:
    """Determine if an action requires human approval."""
    high_risk_actions = ["delete_account", "refund_over_100", "modify_permissions"]
    
    requires_approval = action in high_risk_actions
    reason = f"Action '{action}' is classified as high-risk" if requires_approval else ""
    
    return ActionRequest(
        action=action,
        params=params,
        requires_approval=requires_approval,
        reason=reason
    )


async def request_human_approval(
    session_id: str,
    action_request: ActionRequest,
    thread,
    thread_store
) -> dict:
    """Pause workflow and request human approval."""
    
    # Save thread state
    await thread_store.save_thread(session_id, thread)
    
    # Store pending approval
    approval_id = f"approval-{session_id}-{action_request.action}"
    pending_approvals[approval_id] = {
        "session_id": session_id,
        "action": action_request.action,
        "params": action_request.params,
        "status": ApprovalStatus.PENDING,
        "reason": action_request.reason
    }
    
    # Notify approver (email, Slack, Teams, etc.)
    await notify_approver(approval_id, action_request)
    
    return {
        "status": "pending_approval",
        "approval_id": approval_id,
        "message": f"Action '{action_request.action}' requires approval. ID: {approval_id}"
    }


async def notify_approver(approval_id: str, action_request: ActionRequest):
    """Send notification to human approver."""
    print(f"\n🔔 APPROVAL REQUIRED")
    print(f"   ID: {approval_id}")
    print(f"   Action: {action_request.action}")
    print(f"   Params: {action_request.params}")
    print(f"   Reason: {action_request.reason}")
    print(f"   Approve with: handle_approval('{approval_id}', True)")


async def handle_approval(
    approval_id: str,
    approved: bool,
    agent,
    thread_store
) -> str:
    """Process human approval decision and resume workflow."""
    
    if approval_id not in pending_approvals:
        return f"Approval {approval_id} not found"
    
    approval = pending_approvals[approval_id]
    session_id = approval["session_id"]
    
    # Load the saved thread
    thread = await thread_store.load_thread(session_id, agent)
    
    if approved:
        # Execute the approved action
        result = f"Action '{approval['action']}' executed successfully"
        approval["status"] = ApprovalStatus.APPROVED
        
        # Inform agent of approval
        response = await agent.run(
            f"The action was approved and executed. Result: {result}",
            thread
        )
    else:
        approval["status"] = ApprovalStatus.REJECTED
        response = await agent.run(
            "The action was rejected by the approver. Please suggest alternatives.",
            thread
        )
    
    # Save updated thread
    await thread_store.save_thread(session_id, thread)
    
    # Cleanup
    del pending_approvals[approval_id]
    
    return response.text


if __name__ == "__main__":
    print("Human-in-the-Loop module loaded.")
    print("Use check_if_approval_needed() and handle_approval() for HITL workflows.")
