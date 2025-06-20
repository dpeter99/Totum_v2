using cellarium_backend.Dto;
using cellarium_backend.Services;
using cellarium_backend.Services.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cellarium_backend.Controllers
{
    [Route("api/shopping-list")]
    [ApiController]
    public class ShoppingListController(IShoppingListService shoppingListService, IUserService userService) : ControllerBase
    {
        // GET: api/<ShoppingListController>
        [HttpGet]
        [EndpointName("get-shopping-lists")]
        [EndpointSummary("Get all shopping lists")]
        [EndpointDescription("Returns all shopping lists for the current user")]
        [ProducesResponseType(typeof(IEnumerable<ShoppingListDto>),StatusCodes.Status200OK)]
        public async Task<IEnumerable<ShoppingListDto>> GetAllForUser()
        {
            var currentUser = await userService.GetUser(HttpContext);
            return shoppingListService.GetShoppingLists(currentUser.Id).Select(shoppingList => shoppingList.ToDto());
        }

        // GET api/<ShoppingListController>/5
        [HttpGet("{id}")]
        [EndpointName("get-shopping-list")]
        [EndpointSummary("Get shopping list")]
        [EndpointDescription("Returns a specific shopping list, with it's items")]
        [ProducesResponseType(typeof(ShoppingListWithItemsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id)
        {
            var currentUser = await userService.GetUser(HttpContext);
            var shoppingList = shoppingListService.GetShoppingList(id, currentUser.Id);
            if (shoppingList == null)
            {
                return NotFound();
            }
            return Ok(shoppingList.ToDtoWithItems());
        }

        // POST api/<ShoppingListController>
        [HttpPost]
        [EndpointName("create-shopping-list")]
        [EndpointSummary("Create shopping list")]
        [EndpointDescription("Create a shopping list")]
        [Produces<ShoppingListWithItemsDto>]
        public async Task<IActionResult> Post([FromBody] ShoppingListCreationDto value)
        {
            var currentUser = await userService.GetUser(HttpContext);
            var newList = await shoppingListService.AddShoppingList(value, currentUser.Id);
            if (newList == null)
            {
                return BadRequest();
            }
            return CreatedAtAction(nameof(Get), new{id=newList.Id}, newList);
        }

        // PUT api/<ShoppingListController>/5
        [HttpPut("{id}")]
        [EndpointName("update-shopping-list")]
        [EndpointSummary("Update shopping list")]
        [EndpointDescription("Update a shopping list (only if owned by current user)")]
        [ProducesResponseType(typeof(ShoppingListWithItemsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put(Guid id, [FromBody] ShoppingListCreationDto value)
        {
            var currentUser = await userService.GetUser(HttpContext);
            var updatedList = await shoppingListService.UpdateShoppingList(id, value, currentUser.Id);
            
            if (updatedList == null)
            {
                return NotFound();
            }
            
            return Ok(updatedList.ToDtoWithItems());
        }

        // DELETE api/<ShoppingListController>/5
        [HttpDelete("{id}")]
        [EndpointName("delete-shopping-list")]
        [EndpointSummary("Delete shopping list")]
        [EndpointDescription("Delete a shopping list (only if owned by current user)")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var currentUser = await userService.GetUser(HttpContext);
            var deleteResult = await shoppingListService.DeleteShoppingList(id, currentUser.Id);
            
            if (!deleteResult)
            {
                return NotFound();
            }
            
            return NoContent();
        }
    }
}
