using cellarium_backend.Dto;
using cellarium_backend.Services;
using cellarium_backend.Services.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
            var currentUser = await userService.GetUser(HttpContext);
            var items = await shoppingListItemService.GetItemsForShoppingList(shoppingListId, currentUser.Id);
            
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
            var currentUser = await userService.GetUser(HttpContext);
            var newItem = await shoppingListItemService.AddItemToShoppingList(shoppingListId, value, currentUser.Id);
            
            if (newItem == null)
            {
                return NotFound();
            }
            
            return CreatedAtAction(nameof(Post), new { shoppingListId = newItem.ShoppingListId, id = newItem.Id }, newItem.ToDto());
        }
    }
}