# CELL-001.4: Real-time Updates (SignalR)

## Status: ❌ 0% Complete (New functionality)

### Overview
Implement real-time collaboration features using SignalR to enable live updates when multiple users are working on the same shopping list. Users will see changes instantly without needing to refresh.

## Sub-Tasks

#### CELL-001.4.1: Setup SignalR Infrastructure
**Status:** Not implemented
**Priority:** HIGH - Foundation for real-time features
**Location:** `Program.cs`, new hub classes

**SignalR components needed:**
- Shopping list hub for real-time communication
- Connection management for user groups
- Authentication integration with SignalR
- CORS configuration for frontend access

**Tasks:**
- Install Microsoft.AspNetCore.SignalR NuGet package
- Configure SignalR services in `Program.cs`
- Create `ShoppingListHub` class
- Configure authentication for SignalR connections
- Set up CORS for SignalR endpoint
- Configure connection timeout and retry policies

**Files to create/modify:**
- `Program.cs` (add SignalR configuration)
- `Hubs/ShoppingListHub.cs` (new)
- `Services/IConnectionService.cs` (new - track user connections)
- `Services/ConnectionService.cs` (new)

#### CELL-001.4.2: Implement Shopping List Hub
**Status:** Not implemented
**Priority:** HIGH
**Location:** `Hubs/ShoppingListHub.cs`

**Hub methods to implement:**
- `JoinList(string listId)` - Subscribe to list updates
- `LeaveList(string listId)` - Unsubscribe from list updates
- `ItemAdded(string listId, ShoppingListItemDto item)` - Broadcast new items
- `ItemUpdated(string listId, ShoppingListItemDto item)` - Broadcast item changes
- `ItemDeleted(string listId, string itemId)` - Broadcast item deletions
- `ItemToggled(string listId, string itemId, bool isPurchased)` - Broadcast completion status
- `ListUpdated(string listId, ShoppingListDto list)` - Broadcast list metadata changes
- `UserJoined(string listId, string userName)` - Broadcast user presence
- `UserLeft(string listId, string userName)` - Broadcast user departure

**Authorization requirements:**
- Verify user has permission to join list
- Check collaboration permissions before allowing actions
- Validate user identity on all operations

**Tasks:**
- Create SignalR hub with all required methods
- Implement permission checking in hub methods
- Add proper error handling and logging
- Set up user group management for list subscriptions
- Implement presence tracking for active users

**Files to create:**
- `Hubs/ShoppingListHub.cs`
- `Models/SignalR/HubMessage.cs` (for structured messages)
- `Services/ISignalRAuthorizationService.cs`

#### CELL-001.4.3: Integrate Real-time with API Endpoints
**Status:** Not implemented
**Priority:** HIGH
**Location:** All shopping list controllers and services

**Integration points:**
- When items are added/updated/deleted via REST API, broadcast to SignalR
- When lists are shared/updated, notify all connected users
- When collaborators join/leave, broadcast presence updates
- Ensure REST API and SignalR stay synchronized

**Tasks:**
- Inject `IHubContext<ShoppingListHub>` into services
- Add SignalR broadcasting to all item CRUD operations
- Add broadcasting to list sharing operations
- Add broadcasting to collaboration management
- Implement change detection to avoid unnecessary broadcasts
- Add broadcasting to bulk operations

**Files to modify:**
- `Services/ShoppingListService.cs`
- `Services/ShoppingListItemService.cs`
- `Services/ShoppingListSharingService.cs` (from CELL-001.3)
- `Controllers/ShoppingListController.cs`
- `Controllers/ShoppingListItemController.cs`

#### CELL-001.4.4: Add User Presence Tracking
**Status:** Not implemented
**Priority:** MEDIUM
**Location:** New service and hub integration

**Presence features:**
- Track which users are currently viewing each list
- Show active user indicators in UI
- Track when users join/leave lists
- Handle connection drops gracefully
- Clean up stale presence data

**Presence data model:**
```csharp
public class UserPresence
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public Guid ShoppingListId { get; set; }
    public string ConnectionId { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime LastActivity { get; set; }
}
```

**Tasks:**
- Create presence tracking service
- Implement connection/disconnection handling
- Add presence cleanup for stale connections
- Broadcast presence updates to list members
- Add heartbeat/keepalive mechanism

**Files to create:**
- `Models/SignalR/UserPresence.cs`
- `Services/IPresenceService.cs`
- `Services/PresenceService.cs`
- Update `Hubs/ShoppingListHub.cs`

#### CELL-001.4.5: Add Typing/Editing Indicators
**Status:** Not implemented
**Priority:** LOW
**Location:** Hub and new message types

**Editing indicators:**
- Show when users are typing item names
- Indicate which items are being edited
- Show temporary editing locks to prevent conflicts
- Auto-release locks after timeout

**Hub methods:**
- `StartEditing(string listId, string itemId)` - Lock item for editing
- `StopEditing(string listId, string itemId)` - Release editing lock
- `Typing(string listId, string itemId)` - Show typing indicator
- `StoppedTyping(string listId, string itemId)` - Hide typing indicator

**Tasks:**
- Add editing state tracking
- Implement automatic lock release
- Add typing indicator broadcasting
- Handle concurrent editing conflicts

**Files to modify:**
- `Hubs/ShoppingListHub.cs`
- `Services/IEditingLockService.cs` (new)
- `Services/EditingLockService.cs` (new)

#### CELL-001.4.6: Add Connection Management & Reliability
**Status:** Not implemented
**Priority:** MEDIUM
**Location:** Hub and background services

**Reliability features:**
- Handle network disconnections gracefully
- Implement automatic reconnection on client
- Queue messages for offline users
- Detect and clean up zombie connections
- Add connection retry logic with backoff

**Background services:**
- Connection cleanup service
- Presence data maintenance
- Message queue processing (if implemented)

**Tasks:**
- Implement connection state management
- Add connection cleanup background service
- Handle connection failures and recovery
- Add connection logging and monitoring
- Implement connection limits per user

**Files to create:**
- `Services/ConnectionCleanupService.cs`
- `Services/IConnectionHealthService.cs`
- `BackgroundServices/ConnectionMaintenanceService.cs`

#### CELL-001.4.7: Add Offline Support & Message Queuing
**Status:** Not implemented
**Priority:** LOW
**Location:** New queuing infrastructure

**Offline support:**
- Queue important messages for offline users
- Deliver queued messages when users reconnect
- Handle message ordering and deduplication
- Implement message persistence (Redis/Database)

**Message types to queue:**
- List sharing invitations
- Important item additions
- List ownership changes
- Activity summaries

**Tasks:**
- Implement message queue infrastructure
- Add message persistence layer
- Create message delivery service
- Add message deduplication logic
- Implement message expiration

**Files to create:**
- `Models/SignalR/QueuedMessage.cs`
- `Services/IMessageQueueService.cs`
- `Services/MessageQueueService.cs`
- `BackgroundServices/MessageDeliveryService.cs`

## SignalR Message Structure

### Standard Message Format
```csharp
public class SignalRMessage
{
    public string Type { get; set; } // "ItemAdded", "ItemUpdated", etc.
    public string ListId { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public DateTime Timestamp { get; set; }
    public object Data { get; set; } // Item, List, or other relevant data
}
```

### Message Types
- `ItemAdded` - New item added to list
- `ItemUpdated` - Item properties changed
- `ItemDeleted` - Item removed from list
- `ItemToggled` - Item completion status changed
- `ListUpdated` - List metadata changed
- `UserJoined` - User joined list viewing
- `UserLeft` - User stopped viewing list
- `CollaboratorAdded` - New collaborator added
- `CollaboratorRemoved` - Collaborator removed
- `PermissionChanged` - User permission level changed

## Acceptance Criteria

### CELL-001.4.1 SignalR Infrastructure
- [ ] SignalR is properly configured and running
- [ ] Authentication works with SignalR connections
- [ ] CORS allows frontend connections
- [ ] Connection management is stable
- [ ] Hub endpoint is accessible

### CELL-001.4.2 Shopping List Hub
- [ ] Users can join/leave list subscriptions
- [ ] All CRUD operations broadcast to subscribers
- [ ] Permission checking works in hub methods
- [ ] Error handling provides useful feedback
- [ ] Logging captures all hub activities

### CELL-001.4.3 API Integration
- [ ] REST API operations trigger SignalR broadcasts
- [ ] All item changes are broadcast in real-time
- [ ] List sharing operations are broadcast
- [ ] Bulk operations broadcast efficiently
- [ ] No duplicate messages are sent

### CELL-001.4.4 User Presence
- [ ] Active users are tracked per list
- [ ] Presence updates are broadcast to all users
- [ ] Stale connections are cleaned up
- [ ] Connection drops are handled gracefully
- [ ] Presence data is accurate and current

### CELL-001.4.5 Editing Indicators
- [ ] Typing indicators work for item editing
- [ ] Editing locks prevent conflicts
- [ ] Locks are automatically released
- [ ] Multiple editors are handled properly
- [ ] Visual feedback is clear and responsive

### CELL-001.4.6 Connection Management
- [ ] Connection failures are handled gracefully
- [ ] Reconnection works automatically
- [ ] Zombie connections are cleaned up
- [ ] Connection limits are enforced
- [ ] Performance is maintained under load

### CELL-001.4.7 Offline Support
- [ ] Important messages are queued for offline users
- [ ] Queued messages are delivered on reconnection
- [ ] Message ordering is preserved
- [ ] Duplicate messages are prevented
- [ ] Message storage is efficient and scalable

## Dependencies
- CELL-001.1: Core shopping list endpoints
- CELL-001.2: Shopping list item endpoints
- CELL-001.3: List sharing & collaboration
- SignalR NuGet package
- Redis (optional, for message queuing)

## Estimated Effort
- CELL-001.4.1: 8 hours
- CELL-001.4.2: 16 hours
- CELL-001.4.3: 12 hours
- CELL-001.4.4: 20 hours
- CELL-001.4.5: 12 hours
- CELL-001.4.6: 16 hours
- CELL-001.4.7: 24 hours

**Total: 108 hours**

## Performance Considerations
- Limit number of connections per user
- Implement message batching for high-frequency updates
- Use connection groups efficiently for list subscriptions
- Monitor memory usage for presence tracking
- Implement connection pooling and recycling
- Add rate limiting for message broadcasting

## Security Considerations
- Validate all permissions in hub methods
- Sanitize all user inputs before broadcasting
- Implement rate limiting for message sending
- Log all security-relevant activities
- Prevent message spoofing and injection attacks