using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cellarium_backend;
using cellarium_backend.Dto;
using cellarium_backend.Models;

namespace cellarium_backend.Controllers
{
    [Route("api/shopping-list/item")]
    [ApiController]
    public class ShoppingListItemController(CellariumDbContext context) : ControllerBase
    {
        // GET: api/ShoppinListItemC
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShoppingListItem>>> GetShoppingListItems()
        {
            return await context.ShoppingListItems.ToListAsync();
        }

        // GET: api/ShoppinListItemC/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ShoppingListItem>> GetShoppingListItem(string id)
        {
            var shoppingListItem = await context.ShoppingListItems.FindAsync(id);

            if (shoppingListItem == null)
            {
                return NotFound();
            }

            return shoppingListItem;
        }

        // PUT: api/ShoppinListItemC/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutShoppingListItem(Guid id, ShoppingListItem shoppingListItem)
        {
            if (id != shoppingListItem.Id)
            {
                return BadRequest();
            }

            context.Entry(shoppingListItem).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShoppingListItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ShoppinListItemC
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ShoppingListItem>> PostShoppingListItem(ShoppingListItemCreation createInfo)
        {
            var newItem = context.ShoppingListItems.Add(ShoppingListItem.FromDto(createInfo));
            await context.SaveChangesAsync();

            return CreatedAtAction("GetShoppingListItem", new { id = newItem.Entity.Id }, newItem.Entity);
        }

        // DELETE: api/ShoppinListItemC/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShoppingListItem(string id)
        {
            var shoppingListItem = await context.ShoppingListItems.FindAsync(id);
            if (shoppingListItem == null)
            {
                return NotFound();
            }

            context.ShoppingListItems.Remove(shoppingListItem);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool ShoppingListItemExists(Guid id)
        {
            return context.ShoppingListItems.Any(e => e.Id == id);
        }
    }
}
