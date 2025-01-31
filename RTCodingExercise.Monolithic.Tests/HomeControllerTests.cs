using Microsoft.AspNetCore.Mvc;
using Moq;
using RTCodingExercise.Monolithic.Controllers;
using RTCodingExercise.Monolithic.Models;
using RTCodingExercise.Monolithic.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Xunit;

public class HomeControllerTests
{
    private readonly Mock<IPlateService> _mockPlateService;
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        _mockPlateService = new Mock<IPlateService>();
        _controller = new HomeController(null, _mockPlateService.Object);
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithPlates()
    {
        var plates = new List<Plate>
        {
            new Plate { Id = Guid.NewGuid(), Registration = "A123", PurchasePrice = 100, SalePrice = 320 },
            new Plate { Id = Guid.NewGuid(), Registration = "B456", PurchasePrice = 200, SalePrice = 440 }
        };

        _mockPlateService.Setup(service => service.GetPlatesForPageAsync(1, 20)).ReturnsAsync(plates);

        var result = await _controller.Index(1) as ViewResult;

        Assert.NotNull(result);
        var model = result.Model as List<Plate>;
        Assert.Equal(2, model.Count);
    }

    [Fact]
    public async Task AddPlate_ReturnsRedirectToActionResult_WhenModelStateIsValid()
    {
        // Arrange
        var plate = new Plate
        {
            Id = Guid.NewGuid(),
            Registration = "A123",
            PurchasePrice = 1000,
            SalePrice = 1200
        };

        // Act
        var result = await _controller.AddPlate(plate) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
    }

    [Fact]
    public async Task Index_ShouldReturnEmptyList_WhenNoPlatesExist()
    {
        _mockPlateService.Setup(service => service.GetPlatesForPageAsync(1, 20)).ReturnsAsync(new List<Plate>());

        var result = await _controller.Index(1) as ViewResult;

        Assert.NotNull(result);
        var model = result.Model as List<Plate>;
        Assert.Empty(model); // Empty list if no plates are available
    }

    [Fact]
    public async Task AddPlateAsync_ShouldHandleServiceFailure_WhenExceptionIsThrown()
    {
        var plate = new Plate { Id = Guid.NewGuid(), Registration = "A123", PurchasePrice = 100, SalePrice = 1200 };
        _mockPlateService.Setup(service => service.AddPlateAsync(It.IsAny<Plate>())).ThrowsAsync(new Exception("Service Error"));

        var result = await _controller.AddPlate(plate) as ViewResult;

        Assert.NotNull(result);
        Assert.True(_controller.ModelState.ContainsKey("Error"));
    }
}
