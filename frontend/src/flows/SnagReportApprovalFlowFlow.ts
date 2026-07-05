// Auto-generated flow: SnagReport Approval Flow
// Auto-generated approval flow for SnagReport. Customize email templates and add conditions as needed.
// Resource: SnagReport
// Enabled: true
//
// Nodes:
  // trigger: On SnagReport Submit
  // condition: Status = PendingCRS?
  // approval: SnagReport Approval
  // action: Send Approval Email
  // trigger: On SnagReport Approved
  // action: Send Completion Email
//
// Edges:
  // On SnagReport Submit → Status = PendingCRS?
  // Status = PendingCRS? → SnagReport Approval (true)
  // SnagReport Approval → Send Approval Email
  // On SnagReport Approved → Send Completion Email
//
// This file is for documentation purposes.
// Flow execution is handled by FlowEngine.ts using flowDefinitions.json.

export const FLOW_SNAGREPORT_APPROVAL_FLOW_ID = 'flow-SnagReport-approval';
