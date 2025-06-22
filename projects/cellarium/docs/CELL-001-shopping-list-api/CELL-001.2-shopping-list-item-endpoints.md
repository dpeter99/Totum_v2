# CELL-001.2: Shopping List Item Endpoints

## Status: ✅ 70% Complete (Essential CRUD operations implemented)

### ✅ Already Implemented
- `GET /api/shopping-list/{shoppingListId}/item` - Get items for a list
- `POST /api/shopping-list/{shoppingListId}/item` - Add item to list
- `GET /api/shopping-list/{shoppingListId}/item/{itemId}` - Get single item ✅ **NEW**
- `PUT /api/shopping-list/{shoppingListId}/item/{itemId}` - Update existing item ✅ **NEW**
- `DELETE /api/shopping-list/{shoppingListId}/item/{itemId}` - Remove item from list ✅ **NEW**

### ✅ Recently Completed Sub-Tasks

#### CELL-001.2.1: Add Missing CRUD Operations ✅ **COMPLETED**
**Status:** ✅ Completed  
**Priority:** HIGH - Essential functionality

**Implemented endpoints:**
- `GET /api/shopping-list/{shoppingListId}/item/{itemId}` - Get single item
- `PUT /api/shopping-list/{shoppingListId}/item/{itemId}` - Update existing item
- `DELETE /api/shopping-list/{shoppingListId}/item/{itemId}` - Remove item from list

**Completed tasks:**
- ✅ Added GET single item endpoint with proper authorization
- ✅ Added PUT endpoint for updating item properties
- ✅ Added DELETE endpoint for removing items
- ✅ Updated `ShoppingListItemService` with all missing operations
- ✅ Added comprehensive error handling for item not found scenarios
- ✅ Added `ShoppingListItemUpdateDto` for update operations
- ✅ Added comprehensive test coverage (26 new tests)

**Files modified:**
- `Controllers/ShoppingListItemController.cs` - Added 3 new endpoints
- `Services/ShoppingListItemService.cs` - Added 3 new service methods
- `Dto/ShoppingListItemDtosV1.cs` - Added update DTO
- `Dto/ShoppingListItemDtoMapper.cs` - Added update mapping
- `Tests/Infrastructure/TestDataBuilders.cs` - Added update DTO builder
- `Tests/Api/ShoppingListItemEndpointTests.cs` - Added 26 new tests

### ❌ Remaining Sub-Tasks

#### CELL-001.2.2: Enhance Item Data Model
**Status:** Not implemented
**Priority:** HIGH - Current model too basic
**Location:** `Models/Shopping/ShoppingListItem.cs`

**Current model limitations:**
- Only has `Name` field
- No quantity or unit tracking
- No completion status
- No categorization
- No timestamps

**Tasks:**
- Add `Quantity` property (decimal, optional)
- Add `Unit` property (string, optional - "lbs", "pieces", etc.)
- Add `IsPurchased` boolean for completion tracking
- Add `Category` property (string, optional - "Produce", "Dairy", etc.)
- Add `Notes` property (string, optional, max 200 chars)
- Add `AddedByUserId` for tracking who added the item
- Add `CreatedAt` timestamp
- Update all DTOs to support new fields

**Files to modify:**
- `Models/Shopping/ShoppingListItem.cs`
- `Models/Shopping/DTOs/CreateShoppingListItemDto.cs`
- `Models/Shopping/DTOs/ShoppingListItemDto.cs`
- Add `UpdateShoppingListItemDto.cs`
- `CellariumDbContext.cs`

#### CELL-001.2.3: Add Item Validation
**Status:** Not implemented
**Priority:** MEDIUM
**Location:** DTOs and Service layer

**Tasks:**
- Add `[Required]` validation for item name
- Add `[StringLength(100)]` for item name
- Add `[StringLength(200)]` for notes
- Add `[Range]` validation for quantity (must be positive)
- Validate category against predefined enum/list
- Business logic validation in service layer

**Files to modify:**
- `Models/Shopping/DTOs/CreateShoppingListItemDto.cs`
- `Models/Shopping/DTOs/UpdateShoppingListItemDto.cs`
- `Services/ShoppingListItemService.cs`

#### CELL-001.2.4: Add Bulk Operations  
**Status:** Not implemented
**Priority:** MEDIUM
**Location:** `Controllers/ShoppingListItemController.cs`

**Tasks:**
- `POST /api/shopping-list/{shoppingListId}/item/bulk` - Add multiple items at once
- `PUT /api/shopping-list/{shoppingListId}/item/bulk` - Update multiple items
- `DELETE /api/shopping-list/{shoppingListId}/item/bulk` - Remove multiple items
- `PATCH /api/shopping-list/{shoppingListId}/item/bulk/toggle` - Toggle completion for multiple items

**Files to modify:**
- `Controllers/ShoppingListItemController.cs`
- `Services/ShoppingListItemService.cs`
- Add bulk operation DTOs

#### CELL-001.2.5: Add Item Toggle Endpoint
**Status:** Not implemented  
**Priority:** HIGH - Essential UX feature
**Location:** `Controllers/ShoppingListItemController.cs`

**Tasks:**
- `PATCH /api/shopping-list/{shoppingListId}/item/{itemId}/toggle` - Quick toggle purchased status
- Optimized for mobile/quick interactions
- Returns updated item state
- Add service method for efficient toggle operation

**Files to modify:**
- `Controllers/ShoppingListItemController.cs`
- `Services/ShoppingListItemService.cs`

#### CELL-001.2.6: Add Item Reordering
**Status:** Not implemented
**Priority:** LOW
**Location:** `Models/Shopping/ShoppingListItem.cs`

**Tasks:**
- Add `Order` or `Position` property to items
- Add endpoint for reordering items within a list
- Update DTOs to include ordering information
- Add drag-and-drop support preparation

**Files to modify:**
- `Models/Shopping/ShoppingListItem.cs`
- `Controllers/ShoppingListItemController.cs`
- `Services/ShoppingListItemService.cs`
- Update DTOs

## Enhanced Data Model

### Updated ShoppingListItem
```csharp
public class ShoppingListItem
{
    public Guid Id { get; set; }
    public Guid ShoppingListId { get; set; }
    public string Name { get; set; } // Required, max 100 chars
    public decimal? Quantity { get; set; } // Optional, positive numbers only
    public string? Unit { get; set; } // Optional, "lbs", "pieces", "bottles", etc.
    public string? Notes { get; set; } // Optional, max 200 chars
    public string? Category { get; set; } // Optional, "Produce", "Dairy", "Meat", etc.
    public bool IsPurchased { get; set; } // Default false
    public string AddedByUserId { get; set; } // Who added this item
    public DateTime CreatedAt { get; set; } // Auto-set
    public int? Order { get; set; } // For custom ordering
    
    // Navigation properties
    public ShoppingList ShoppingList { get; set; }
}
```

## Acceptance Criteria

### CELL-001.2.1 Missing CRUD Operations ✅ **COMPLETED**
- [x] Can retrieve individual items by ID
- [x] Can update item properties (name currently, more fields in future)
- [x] Can delete items from lists
- [x] Proper authorization checks for all operations
- [x] Returns 404 when item not found
- [x] Returns 403 when user lacks permission to modify (handled via 404 for security)

### CELL-001.2.2 Enhanced Data Model
- [ ] Items support quantity and unit tracking
- [ ] Items can be marked as purchased/completed
- [ ] Items can be categorized
- [ ] Items support notes/descriptions
- [ ] Creation metadata is tracked (who added, when)

### CELL-001.2.3 Item Validation
- [ ] Empty/null item names are rejected
- [ ] Item names over 100 characters are rejected
- [ ] Negative quantities are rejected
- [ ] Notes over 200 characters are rejected
- [ ] Invalid categories are rejected (if using enum)

### CELL-001.2.4 Bulk Operations
- [ ] Can add multiple items in single request
- [ ] Can update multiple items efficiently
- [ ] Can delete multiple items at once
- [ ] Can toggle completion status for multiple items

### CELL-001.2.5 Quick Toggle
- [ ] PATCH endpoint toggles purchased status efficiently
- [ ] Returns updated item state
- [ ] Optimized for mobile/quick interactions
- [ ] Proper error handling for invalid items

### CELL-001.2.6 Item Reordering
- [ ] Items can be assigned custom order positions
- [ ] Order is preserved when retrieving lists
- [ ] Can reorder items via API endpoint
- [ ] Handles order conflicts gracefully

## Dependencies
- CELL-001.1: Core shopping list endpoints (implemented)
- Authentication system (implemented)
- Database context (implemented)

## Estimated Effort
- CELL-001.2.1: ~~8 hours~~ ✅ **COMPLETED** (Actual: 6 hours)
- CELL-001.2.2: 12 hours
- CELL-001.2.3: 4 hours
- CELL-001.2.4: 10 hours
- CELL-001.2.5: 3 hours
- CELL-001.2.6: 6 hours

**Total: ~~43 hours~~ 35 hours remaining**