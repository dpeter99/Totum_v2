using cellarium_backend.Dto;
using cellarium_backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cellarium_backend.Controllers
{
    [Route("api/shopping-list")]
    [ApiController]
    public class ShoppingListController(IShoppingListService shoppingListService) : ControllerBase
    {
        // GET: api/<ShoppingListController>
        [HttpGet]
        [EndpointName("get-shopping-lists")]
        [EndpointSummary("Get all shopping lists")]
        [EndpointDescription("Returns all shopping lists for the current user")]
        [ProducesResponseType(typeof(IEnumerable<ShoppingListDto>),StatusCodes.Status200OK)]
        public IEnumerable<ShoppingListDto> GetAllForUser()
        {
            return shoppingListService.GetShoppingLists().Select(shoppingList => shoppingList.ToDto());
        }

        // GET api/<ShoppingListController>/5
        [HttpGet("{id}")]
        [EndpointName("get-shopping-list")]
        [EndpointSummary("Get shopping list")]
        [EndpointDescription("Returns a specific shopping list, with it's items")]
        [ProducesResponseType(typeof(ShoppingListWithItemsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(Guid id)
        {
            var shoppingList = shoppingListService.GetShoppingList(id);
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
            var newList = await shoppingListService.AddShoppingList(value);
            if (newList == null)
            {
                return BadRequest();
            }
            return CreatedAtAction(nameof(Get), new{id=newList.Id}, newList);
        }

        // PUT api/<ShoppingListController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ShoppingListController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
