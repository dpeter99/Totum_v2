# CELL-001.3: List Sharing & Collaboration Endpoints

## Status: ❌ 0% Complete (New functionality)

### Overview
Enable users to share shopping lists with other users and collaborate in real-time. This requires new data models, endpoints, and business logic for permission management.

## Sub-Tasks

#### CELL-001.3.1: Create Collaboration Data Models
**Status:** Not implemented
**Priority:** HIGH - Foundation for all collaboration features
**Location:** `Models/Shopping/`

**New models needed:**
```csharp
// Permission levels for collaborators
public enum CollaboratorRole
{
    Owner = 0,    // Full control, can delete list, manage collaborators
    Editor = 1,   // Can add/edit/delete items, cannot manage collaborators  
    Viewer = 2    // Read-only access
}

// Collaboration relationship
public class ShoppingListCollaborator  
{
    public Guid Id { get; set; }
    public Guid ShoppingListId { get; set; }
    public string UserId { get; set; } // Collaborator's user ID
    public CollaboratorRole Role { get; set; }
    public DateTime SharedAt { get; set; }
    public string SharedByUserId { get; set; } // Who shared the list
    public bool IsActive { get; set; } // For soft-delete of collaborations
    
    // Navigation properties
    public ShoppingList ShoppingList { get; set; }
}
```

**Tasks:**
- Create `CollaboratorRole` enum
- Create `ShoppingListCollaborator` entity model
- Update `ShoppingList` model to include `Collaborators` collection
- Configure entity relationships in `CellariumDbContext`
- Create DTOs for collaboration operations

**Files to create/modify:**
- `Models/Shopping/CollaboratorRole.cs`
- `Models/Shopping/ShoppingListCollaborator.cs`
- `Models/Shopping/ShoppingList.cs` (add Collaborators property)
- `Models/Shopping/DTOs/CollaboratorDto.cs`
- `Models/Shopping/DTOs/ShareListDto.cs`
- `CellariumDbContext.cs`

#### CELL-001.3.2: Create List Sharing Endpoints
**Status:** Not implemented
**Priority:** HIGH
**Location:** New controller or extend existing

**Required endpoints:**
- `POST /api/shopping-list/{listId}/share` - Share list with user
- `GET /api/shopping-list/{listId}/collaborators` - Get list collaborators
- `PUT /api/shopping-list/{listId}/collaborators/{userId}` - Update user permissions
- `DELETE /api/shopping-list/{listId}/collaborators/{userId}` - Remove collaborator

**Tasks:**
- Create sharing endpoint that accepts user email/username
- Implement user lookup by email/username (integrate with Identity system)
- Add permission validation (only owners can share)
- Create collaborator management endpoints
- Add proper authorization for collaboration operations
- Handle edge cases (sharing with self, duplicate shares, etc.)

**Files to create/modify:**
- `Controllers/ShoppingListSharingController.cs` (new)
- `Services/ShoppingListSharingService.cs` (new)
- `Services/IUserLookupService.cs` (new - for finding users by email)

#### CELL-001.3.3: Create Shared Lists Discovery
**Status:** Not implemented  
**Priority:** HIGH
**Location:** Extend existing or new controller

**Required endpoints:**
- `GET /api/shopping-list/shared-with-me` - Get lists user has access to
- `GET /api/shopping-list/shared-by-me` - Get lists user has shared
- `POST /api/shopping-list/{listId}/accept` - Accept list sharing invitation
- `POST /api/shopping-list/{listId}/decline` - Decline list sharing invitation

**Tasks:**
- Modify existing list retrieval to include shared lists
- Add filtering options for owned vs shared vs all lists
- Implement invitation acceptance workflow  
- Add sharing status indicators
- Update list DTOs to include sharing information

**Files to modify:**
- `Controllers/ShoppingListController.cs`
- `Services/ShoppingListService.cs`
- `Models/Shopping/DTOs/ShoppingListDto.cs`

#### CELL-001.3.4: Implement Permission Authorization
**Status:** Not implemented
**Priority:** HIGH - Security critical
**Location:** Service layer and authorization filters

**Authorization matrix:**

| Operation | Owner | Editor | Viewer |
|-----------|-------|--------|--------|
| View list | ✓ | ✓ | ✓ |
| Add items | ✓ | ✓ | ❌ |
| Edit items | ✓ | ✓ | ❌ |
| Delete items | ✓ | ✓ | ❌ |
| Delete list | ✓ | ❌ | ❌ |
| Share list | ✓ | ❌ | ❌ |
| Manage collaborators | ✓ | ❌ | ❌ |

**Tasks:**
- Create `ICollaborationAuthorizationService` for permission checks
- Update all existing endpoints to check collaboration permissions
- Create custom authorization attributes for shopping lists
- Add permission checking to all item operations
- Handle authorization failures with proper HTTP responses

**Files to create/modify:**
- `Services/ICollaborationAuthorizationService.cs` (new)
- `Services/CollaborationAuthorizationService.cs` (new)
- `Attributes/RequireListPermissionAttribute.cs` (new)
- Update all controllers to use permission checking

#### CELL-001.3.5: Add Activity Tracking
**Status:** Not implemented
**Priority:** MEDIUM
**Location:** New models and services

**Activity tracking features:**
- Track who added/modified/deleted items
- Track when lists were shared
- Track when users joined/left collaborations
- Activity feed for list changes

**New models:**
```csharp
public class ShoppingListActivity
{
    public Guid Id { get; set; }
    public Guid ShoppingListId { get; set; }
    public string UserId { get; set; }
    public ActivityType Type { get; set; } // ItemAdded, ItemCompleted, UserJoined, etc.
    public string? ItemName { get; set; } // For item-related activities
    public string? AdditionalData { get; set; } // JSON for extra context
    public DateTime Timestamp { get; set; }
    
    public ShoppingList ShoppingList { get; set; }
}

public enum ActivityType
{
    ItemAdded, ItemCompleted, ItemDeleted, ItemUpdated,
    ListShared, UserJoined, UserLeft, ListRenamed
}
```

**Tasks:**
- Create activity tracking models
- Add activity logging to all relevant operations
- Create activity feed endpoints
- Add activity cleanup/retention policies

**Files to create:**
- `Models/Shopping/ShoppingListActivity.cs`
- `Models/Shopping/ActivityType.cs`
- `Services/IActivityTrackingService.cs`
- `Services/ActivityTrackingService.cs`
- `Controllers/ShoppingListActivityController.cs`

#### CELL-001.3.6: Add Invitation System
**Status:** Not implemented
**Priority:** MEDIUM
**Location:** New models and notification system

**Invitation workflow:**
1. User shares list with email address
2. System creates pending invitation
3. Target user receives notification (email/in-app)
4. User can accept/decline invitation
5. Accepted invitations become active collaborations

**Tasks:**
- Create invitation data model
- Implement email-based invitation system
- Add invitation acceptance/decline endpoints
- Add invitation expiration handling
- Integrate with notification system

**Files to create:**
- `Models/Shopping/ShoppingListInvitation.cs`
- `Services/IInvitationService.cs`
- `Services/InvitationService.cs`
- `Controllers/InvitationController.cs`

## Enhanced Data Models

### Updated ShoppingList
```csharp
public class ShoppingList
{
    public Guid Id { get; set; }
    public string UserId { get; set; } // Original owner
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Collections
    public List<ShoppingListItem> Items { get; set; }
    public List<ShoppingListCollaborator> Collaborators { get; set; }
    public List<ShoppingListActivity> Activities { get; set; }
}
```

## Acceptance Criteria

### CELL-001.3.1 Collaboration Models
- [ ] CollaboratorRole enum supports Owner/Editor/Viewer levels
- [ ] ShoppingListCollaborator model tracks all required relationships
- [ ] Database relationships properly configured
- [ ] DTOs support all collaboration operations

### CELL-001.3.2 Sharing Endpoints
- [ ] Users can share lists by email/username
- [ ] Only list owners can share lists
- [ ] Users can be found by email address
- [ ] Collaborators can be added/removed/updated
- [ ] Proper error handling for invalid users/permissions

### CELL-001.3.3 Shared Lists Discovery
- [ ] Users can see lists shared with them
- [ ] Users can see lists they've shared
- [ ] Shared lists appear in main list view
- [ ] Sharing status is clearly indicated
- [ ] Invitation acceptance workflow works

### CELL-001.3.4 Permission Authorization
- [ ] All operations properly check permissions
- [ ] Viewers cannot modify lists or items
- [ ] Editors cannot manage collaborators or delete lists
- [ ] Only owners can share lists and manage collaborators
- [ ] Unauthorized operations return 403 Forbidden

### CELL-001.3.5 Activity Tracking
- [ ] All significant actions are logged
- [ ] Activity feed shows chronological changes
- [ ] Activities include user attribution
- [ ] Activity data is properly structured
- [ ] Old activities can be cleaned up

### CELL-001.3.6 Invitation System
- [ ] Email invitations are sent successfully
- [ ] Users can accept/decline invitations
- [ ] Expired invitations are handled properly
- [ ] Invitation status is tracked
- [ ] Integration with notification system works

## Dependencies
- CELL-001.1: Core shopping list endpoints
- CELL-001.2: Shopping list item endpoints
- Authentication/Identity system integration
- Email sending service (for invitations)

## Estimated Effort
- CELL-001.3.1: 16 hours
- CELL-001.3.2: 20 hours
- CELL-001.3.3: 12 hours
- CELL-001.3.4: 24 hours
- CELL-001.3.5: 16 hours
- CELL-001.3.6: 20 hours

**Total: 108 hours**

## Security Considerations
- Always validate user permissions before operations
- Prevent information leakage through unauthorized access
- Implement rate limiting for sharing operations
- Validate all user inputs for sharing operations
- Log all permission-related activities for audit