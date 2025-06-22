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