using Microsoft.AspNetCore.Mvc;
using RTCodingExercise.Monolithic.Models;
using RTCodingExercise.Monolithic.Services;
using System.Diagnostics;

namespace RTCodingExercise.Monolithic.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPlateService _plateService;

        public HomeController(ILogger<HomeController> logger, IPlateService plateService)
        {
            _logger = logger;
            _plateService = plateService;
        }

        public async Task<IActionResult> Index(int page = 1, string sortOrder = "asc", string? filter = null)
        {
           const int pageSize = 20;
           var plates = await _plateService.GetPlatesForPageAsync(page, pageSize, sortOrder, filter);
           ViewBag.Page = page;
           ViewBag.SortOrder = sortOrder;
           ViewBag.Filter = filter; 

            return View(plates);
        }

        public IActionResult AddPlate()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPlate(Plate plate)
        {
            if (!ModelState.IsValid)
            {
                return View(plate);
            }

            try
            {
                await _plateService.AddPlateAsync(plate);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", "An error occurred while adding the plate.");
                return View(plate);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleReservation(Guid plateId)
        {
            var plateToUpdate = await _plateService.GetPlateByIdAsync(plateId);
            if (plateToUpdate != null)
            {
                bool newStatus = !plateToUpdate.Reserved;
                await _plateService.SetPlateReservationStatusAsync(plateId, newStatus);
                _logger.LogInformation($"Plate {plateToUpdate?.Registration} has been marked as {(newStatus ? "reserved" : "unreserved")}");
            }

            return RedirectToAction("Index");
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}