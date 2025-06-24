using cellarium_backend.Features.ShoppingLists.Dto;
using cellarium_backend.Features.ShoppingLists.Services;
using cellarium_backend.Shared.Services.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace cellarium_backend.Features.ShoppingLists.Controllers
{
    [Route("api/shopping-list/{shoppingListId}/items")]
    [ApiController]
    public class ShoppingListItemsBulkController(IShoppingListItemService shoppingListItemService, IUserService userService) : ControllerBase
    {
        // POST api/shopping-list/{shoppingListId}/items
        [HttpPost]
        [EndpointName("add-multiple-items-to-shopping-list")]
        [EndpointSummary("Add multiple items to shopping list")]
        [EndpointDescription("Add multiple items to a shopping list in a single operation (only if list is owned by current user)")]
        [ProducesResponseType(typeof(BulkOperationResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PostMultiple(Guid shoppingListId, [FromBody] BulkShoppingListItemCreationDto bulkDto)
        {
            using var span = ActivityHelper.Source.StartActivity("add-multiple-shopping-list-items");
            span?.AddTag("shopping_list.id", shoppingListId.ToString());
            span?.AddTag("items.count", bulkDto.Items.Count.ToString());
            
            var currentUser = await userService.GetUser(HttpContext);
            span?.AddTag("user.id", currentUser.Id);
            
            var result = await shoppingListItemService.AddMultipleItemsToShoppingList(shoppingListId, bulkDto, currentUser.Id);
            
            span?.AddTag("result.success_count", result.SuccessCount.ToString());
            span?.AddTag("result.failure_count", result.FailureCount.ToString());
            
            if (result.FailureCount == bulkDto.Items.Count && result.Errors.Any(e => e.Contains("not found or access denied")))
            {
                span?.AddTag("result", "not_found");
                return NotFound();
            }
            
            span?.AddTag("result", "completed");
            return Ok(result);
        }

        // PUT api/shopping-list/{shoppingListId}/items
        [HttpPut]
        [EndpointName("update-multiple-shopping-list-items")]
        [EndpointSummary("Update multiple items in shopping list")]
        [EndpointDescription("Update multiple items in a shopping list in a single operation (only if list is owned by current user)")]
        [ProducesResponseType(typeof(BulkOperationResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutMultiple(Guid shoppingListId, [FromBody] BulkShoppingListItemUpdateDto bulkDto)
        {
            using var span = ActivityHelper.Source.StartActivity("update-multiple-shopping-list-items");
            span?.AddTag("shopping_list.id", shoppingListId.ToString());
            span?.AddTag("items.count", bulkDto.Updates.Count.ToString());
            
            var currentUser = await userService.GetUser(HttpContext);
            span?.AddTag("user.id", currentUser.Id);
            
            var result = await shoppingListItemService.UpdateMultipleItems(shoppingListId, bulkDto, currentUser.Id);
            
            span?.AddTag("result.success_count", result.SuccessCount.ToString());
            span?.AddTag("result.failure_count", result.FailureCount.ToString());
            
            if (result.FailureCount == bulkDto.Updates.Count && result.Errors.Any(e => e.Contains("not found or access denied")))
            {
                span?.AddTag("result", "not_found");
                return NotFound();
            }
            
            span?.AddTag("result", "completed");
            return Ok(result);
        }

        // DELETE api/shopping-list/{shoppingListId}/items
        [HttpDelete]
        [EndpointName("delete-multiple-shopping-list-items")]
        [EndpointSummary("Delete multiple items from shopping list")]
        [EndpointDescription("Delete multiple items from a shopping list in a single operation (only if list is owned by current user)")]
        [ProducesResponseType(typeof(BulkOperationResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMultiple(Guid shoppingListId, [FromBody] BulkShoppingListItemDeleteDto bulkDto)
        {
            using var span = ActivityHelper.Source.StartActivity("delete-multiple-shopping-list-items");
            span?.AddTag("shopping_list.id", shoppingListId.ToString());
            span?.AddTag("items.count", bulkDto.ItemIds.Count.ToString());
            
            var currentUser = await userService.GetUser(HttpContext);
            span?.AddTag("user.id", currentUser.Id);
            
            var result = await shoppingListItemService.DeleteMultipleItems(shoppingListId, bulkDto, currentUser.Id);
            
            span?.AddTag("result.success_count", result.SuccessCount.ToString());
            span?.AddTag("result.failure_count", result.FailureCount.ToString());
            
            if (result.FailureCount == bulkDto.ItemIds.Count && result.Errors.Any(e => e.Contains("not found or access denied")))
            {
                span?.AddTag("result", "not_found");
                return NotFound();
            }
            
            span?.AddTag("result", "completed");
            return Ok(result);
        }

        // PATCH api/shopping-list/{shoppingListId}/items/reorder
        [HttpPatch("reorder")]
        [EndpointName("reorder-shopping-list-items")]
        [EndpointSummary("Reorder items in shopping list")]
        [EndpointDescription("Reorder multiple items in a shopping list by setting new order positions (only if list is owned by current user)")]
        [ProducesResponseType(typeof(BulkOperationResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReorderItems(Guid shoppingListId, [FromBody] BulkItemReorderDto reorderDto)
        {
            using var span = ActivityHelper.Source.StartActivity("reorder-shopping-list-items");
            span?.AddTag("shopping_list.id", shoppingListId.ToString());
            span?.AddTag("items.count", reorderDto.Reorders.Count.ToString());
            
            var currentUser = await userService.GetUser(HttpContext);
            span?.AddTag("user.id", currentUser.Id);
            
            var result = await shoppingListItemService.ReorderItems(shoppingListId, reorderDto, currentUser.Id);
            
            span?.AddTag("result.success_count", result.SuccessCount.ToString());
            span?.AddTag("result.failure_count", result.FailureCount.ToString());
            
            if (result.FailureCount == reorderDto.Reorders.Count && result.Errors.Any(e => e.Contains("not found or access denied")))
            {
                span?.AddTag("result", "not_found");
                return NotFound();
            }
            
            span?.AddTag("result", "completed");
            return Ok(result);
        }
    }
}