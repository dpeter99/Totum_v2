using cellarium_backend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace cellarium_backend.Tests;

public class Tests
{
    [Test]
    public async Task Index_ReturnsAViewResult_WithAListOfBrainstormSessions()
    {
        // Arrange
        var controller = new ShoppingListController();

        // Act
        var result = controller.GetAllForUser();

    }
}