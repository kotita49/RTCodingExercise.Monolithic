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

        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 20;
           var plates = await _plateService.GetPlatesForPageAsync(page, pageSize);
            ViewBag.Page = page;

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