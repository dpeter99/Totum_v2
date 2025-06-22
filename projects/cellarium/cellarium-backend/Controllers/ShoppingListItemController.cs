using cellarium_backend.Dto;
using cellarium_backend.Services;
using cellarium_backend.Services.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace cellarium_backend.Controllers
{
    [Route("api/shopping-list/{shoppingListId}/item")]
    [ApiController]
    public class ShoppingListItemController(IShoppingListItemService shoppingListItemService, IUserService userService) : ControllerBase
    {
        // GET api/shopping-list/{shoppingListId}/item
        [HttpGet]
        [EndpointName("get-items-for-shopping-list")]
        [EndpointSummary("Get items for shopping list")]
        [EndpointDescription("Get all items for a shopping list (only if list is owned by current user)")]
        [ProducesResponseType(typeof(IEnumerable<ShoppingListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid shoppingListId)
        {
            using var span = ActivityHelper.Source.StartActivity("get-shopping-list-items");
            span?.AddTag("shopping_list.id", shoppingListId.ToString());
            
            var currentUser = await userService.GetUser(HttpContext);
            span?.AddTag("user.id", currentUser.Id);
            
            var items = await shoppingListItemService.GetItemsForShoppingList(shoppingListId, currentUser.Id);
            span?.AddTag("items.count", items.Count().ToString());
            
            return Ok(items.Select(item => item.ToDto()));
        }

        // GET api/shopping-list/{shoppingListId}/item/{itemId}
        [HttpGet("{itemId}")]
        [EndpointName("get-shopping-list-item")]
        [EndpointSummary("Get single item from shopping list")]
        [EndpointDescription("Get a specific item from a shopping list (only if list is owned by current user)")]
        [ProducesResponseType(typeof(ShoppingListItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid shoppingListId, Guid itemId)
        {
            using var span = ActivityHelper.Source.StartActivity("get-shopping-list-item");
            span?.AddTag("shopping_list.id", shoppingListId.ToString());
            span?.AddTag("item.id", itemId.ToString());
            
            var currentUser = await userService.GetUser(HttpContext);
            span?.AddTag("user.id", currentUser.Id);
            
            var item = await shoppingListItemService.GetItemById(shoppingListId, itemId, currentUser.Id);
            
            if (item == null)
            {
                span?.AddTag("result", "not_found");
                return NotFound();
            }
            
            span?.AddTag("result", "success");
            span?.AddTag("item.name", item.Name);
            
            return Ok(item.ToDto());
        }

        // PUT api/shopping-list/{shoppingListId}/item/{itemId}
        [HttpPut("{itemId}")]
        [EndpointName("update-shopping-list-item")]
        [EndpointSummary("Update item in shopping list")]
        [EndpointDescription("Update a specific item in a shopping list (only if list is owned by current user)")]
        [ProducesResponseType(typeof(ShoppingListItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put(Guid shoppingListId, Guid itemId, [FromBody] ShoppingListItemUpdateDto updateDto)
        {
            using var span = ActivityHelper.Source.StartActivity("update-shopping-list-item");
            span?.AddTag("shopping_list.id", shoppingListId.ToString());
            span?.AddTag("item.id", itemId.ToString());
            span?.AddTag("item.name", updateDto.Name);
            
            var currentUser = await userService.GetUser(HttpContext);
            span?.AddTag("user.id", currentUser.Id);
            
            var updatedItem = await shoppingListItemService.UpdateItem(shoppingListId, itemId, updateDto, currentUser.Id);
            
            if (updatedItem == null)
            {
                span?.AddTag("result", "not_found");
                return NotFound();
            }
            
            span?.AddTag("result", "success");
            
            return Ok(updatedItem.ToDto());
        }

        // DELETE api/shopping-list/{shoppingListId}/item/{itemId}
        [HttpDelete("{itemId}")]
        [EndpointName("delete-shopping-list-item")]
        [EndpointSummary("Delete item from shopping list")]
        [EndpointDescription("Delete a specific item from a shopping list (only if list is owned by current user)")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid shoppingListId, Guid itemId)
        {
            using var span = ActivityHelper.Source.StartActivity("delete-shopping-list-item");
            span?.AddTag("shopping_list.id", shoppingListId.ToString());
            span?.AddTag("item.id", itemId.ToString());
            
            var currentUser = await userService.GetUser(HttpContext);
            span?.AddTag("user.id", currentUser.Id);
            
            var deleted = await shoppingListItemService.DeleteItem(shoppingListId, itemId, currentUser.Id);
            
            if (!deleted)
            {
                span?.AddTag("result", "not_found");
                return NotFound();
            }
            
            span?.AddTag("result", "success");
            
            return NoContent();
        }

        // POST api/shopping-list/{shoppingListId}/item
        [HttpPost]
        [EndpointName("add-item-to-shopping-list")]
        [EndpointSummary("Add item to shopping list")]
        [EndpointDescription("Add a new item to a shopping list (only if list is owned by current user)")]
        [ProducesResponseType(typeof(ShoppingListItemDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Post(Guid shoppingListId, [FromBody] ShoppingListItemCreationDto value)
        {
            using var span = ActivityHelper.Source.StartActivity("add-shopping-list-item");
            span?.AddTag("shopping_list.id", shoppingListId.ToString());
            span?.AddTag("item.name", value.Name);
            
            var currentUser = await userService.GetUser(HttpContext);
            span?.AddTag("user.id", currentUser.Id);
            
            var newItem = await shoppingListItemService.AddItemToShoppingList(shoppingListId, value, currentUser.Id);
            
            if (newItem == null)
            {
                span?.AddTag("result", "not_found");
                return NotFound();
            }
            
            span?.AddTag("result", "success");
            span?.AddTag("item.id", newItem.Id.ToString());
            
            return CreatedAtAction(nameof(Post), new { shoppingListId = newItem.ShoppingListId, id = newItem.Id }, newItem.ToDto());
        }
    }
}