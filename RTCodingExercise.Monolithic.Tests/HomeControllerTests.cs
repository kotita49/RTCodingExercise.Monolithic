using Microsoft.AspNetCore.Mvc;
using Moq;
using RTCodingExercise.Monolithic.Controllers;
using RTCodingExercise.Monolithic.Models;
using RTCodingExercise.Monolithic.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Xunit;
using System.Linq;
using Microsoft.Extensions.Logging;

public class HomeControllerTests
{
    private readonly Mock<IPlateService> _mockPlateService;
    private readonly Mock<ILogger<HomeController>> _mockLogger;
    private readonly HomeController _controller;
    
    public HomeControllerTests()
    {
        _mockPlateService = new Mock<IPlateService>();
        _mockLogger = new Mock<ILogger<HomeController>>();
        _controller = new HomeController(_mockLogger.Object, _mockPlateService.Object);
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithPlates()
    {
        var plates = new List<Plate>
        {
            new Plate { Id = Guid.NewGuid(), Registration = "A123", PurchasePrice = 100, SalePrice = 320 },
            new Plate { Id = Guid.NewGuid(), Registration = "B456", PurchasePrice = 200, SalePrice = 440 }
        };

        _mockPlateService.Setup(service => service.GetPlatesForPageAsync(1, 20, "asc", null)).ReturnsAsync(plates);

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
        _mockPlateService.Setup(service => service.GetPlatesForPageAsync(1, 20, "asc", null)).ReturnsAsync(new List<Plate>());

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

    [Fact]
    public async Task Index_ShouldReturnPaginatedPlates()
    {
        // Arrange
        var plates = new List<Plate>
    {
        new Plate { Id = Guid.NewGuid(), Registration = "A123", SalePrice = 100 },
        new Plate { Id = Guid.NewGuid(), Registration = "B456", SalePrice = 200 },
        new Plate { Id = Guid.NewGuid(), Registration = "C789", SalePrice = 300 }
    };

        _mockPlateService.Setup(service => service.GetPlatesForPageAsync(2, 20, "asc", null)).ReturnsAsync(plates.Skip(20).Take(20).ToList());

        // Act
        var result = await _controller.Index(2) as ViewResult;

        // Assert
        Assert.NotNull(result);
        var model = result.Model as List<Plate>;
        Assert.Empty(model); 
    }

    [Fact]
    public async Task GetPlatesForPageAsync_ShouldReturnPlatesSortedByPriceAsc()
    {
        // Arrange
        var plates = new List<Plate>
    {
        new Plate { Id = Guid.NewGuid(), Registration = "A123", SalePrice = 300 },
        new Plate { Id = Guid.NewGuid(), Registration = "B456", SalePrice = 100 },
        new Plate { Id = Guid.NewGuid(), Registration = "C789", SalePrice = 200 }
    };

        _mockPlateService.Setup(service => service.GetPlatesForPageAsync(1, 20, "asc", null)).ReturnsAsync(plates.OrderBy(p => p.SalePrice).ToList());
        
        // Act - Ascending
        var resultAsc = await _controller.Index(1) as ViewResult;
        var modelAsc = resultAsc.Model as List<Plate>;

        // Assert - Ascending
        Assert.NotNull(modelAsc);
        Assert.Equal(100, modelAsc[0].SalePrice);
        Assert.Equal(200, modelAsc[1].SalePrice);
        Assert.Equal(300, modelAsc[2].SalePrice);       
    }

    [Fact]
    public async Task GetPlatesForPageAsync_ShouldReturnPlatesSortedByPriceDesc()
    {
        // Arrange
        var plates = new List<Plate>
    {
        new Plate { Id = Guid.NewGuid(), Registration = "A123", SalePrice = 500 },
        new Plate { Id = Guid.NewGuid(), Registration = "B456", SalePrice = 100 },
        new Plate { Id = Guid.NewGuid(), Registration = "C789", SalePrice = 700 }
    };

        _mockPlateService.Setup(service => service.GetPlatesForPageAsync(1, 20, "desc", null)).ReturnsAsync(plates.OrderByDescending(p => p.SalePrice).ToList());

        // Act - Descending
        var resultDesc = await _controller.Index(1, "desc") as ViewResult;
        var modelDesc = resultDesc.Model as List<Plate>;

        // Assert - Descending
        Assert.NotNull(modelDesc);
        Assert.Equal(700, modelDesc[0].SalePrice);
        Assert.Equal(500, modelDesc[1].SalePrice);
        Assert.Equal(100, modelDesc[2].SalePrice);
    }

    [Fact]
    public async Task Index_ShouldReturnFilteredPlates_WhenFilterIsProvided()
    {
        // Arrange
        var plates = new List<Plate>
    {
        new Plate { Id = Guid.NewGuid(), Registration = "A123", SalePrice = 100, Reserved = false },
        new Plate { Id = Guid.NewGuid(), Registration = "B456", SalePrice = 200, Reserved = false },
        new Plate { Id = Guid.NewGuid(), Registration = "A333", SalePrice = 400, Reserved = true }
    };

        _mockPlateService.Setup(service => service.GetPlatesForPageAsync(1, 20, "asc", "A")).ReturnsAsync(plates.Where(p => p.Registration.Contains("A123")).ToList());

        // Act
        var result = await _controller.Index(1, "asc", "A") as ViewResult;

        // Assert
        Assert.NotNull(result);
        var model = result.Model as List<Plate>;
        Assert.Single(model);
        Assert.Equal("A123", model[0].Registration);
    }

    [Fact]
    public async Task ToggleReservation_ShouldToggleReservationStatus()
    {
        // Arrange
        var plateId = Guid.NewGuid();
        var plate = new Plate { Id = plateId, Registration = "A123", Reserved = false };

        _mockPlateService.Setup(service => service.GetPlateByIdAsync(plateId)).ReturnsAsync(plate);
        _mockPlateService.Setup(service => service.SetPlateReservationStatusAsync(plateId, true)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ToggleReservation(plateId) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
    }

    [Fact]
    public async Task GetPlatesForPageAsync_ShouldReturnOnlyPlatesForSale()
    {
        // Arrange
        var plates = new List<Plate>
    {
        new Plate { Id = Guid.NewGuid(), Registration = "A123", SalePrice = 100, Reserved = false },
        new Plate { Id = Guid.NewGuid(), Registration = "B456", SalePrice = 200, Reserved = true },
        new Plate { Id = Guid.NewGuid(), Registration = "C789", SalePrice = 300, Reserved = false }
    };

        _mockPlateService.Setup(service => service.GetPlatesForPageAsync(1, 20, "asc", null))
                        .ReturnsAsync(plates.Where(p => !p.Reserved).ToList());

        // Act
        var result = await _controller.Index(1) as ViewResult;
        var model = result.Model as List<Plate>;

        // Assert
        Assert.NotNull(model);
        Assert.Equal(2, model.Count);
        Assert.DoesNotContain(model, p => p.Reserved); 
    }

    [Fact]
    public async Task GetPlatesForPageAsync_ShouldReturnEmptyList_WhenAllPlatesAreReserved()
    {
        // Arrange
        var plates = new List<Plate>
    {
        new Plate { Id = Guid.NewGuid(), Registration = "A123", SalePrice = 100, Reserved = true },
        new Plate { Id = Guid.NewGuid(), Registration = "B456", SalePrice = 200, Reserved = true }
    };

        _mockPlateService.Setup(service => service.GetPlatesForPageAsync(1, 20, "asc", null))
                        .ReturnsAsync(new List<Plate>());

        // Act
        var result = await _controller.Index(1) as ViewResult;
        var model = result.Model as List<Plate>;

        // Assert
        Assert.NotNull(model);
        Assert.Empty(model); 
    }
}


