using cellarium_backend.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cellarium_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingListController : ControllerBase
    {
        // GET: api/<ShoppingListController>
        [HttpGet]
        [EndpointSummary("Returns all shopping lists for the current user")]
        [Produces<ShoppingListDto>]
        public IEnumerable<string> GetAllForUser()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<ShoppingListController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ShoppingListController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
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
