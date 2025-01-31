using RTCodingExercise.Monolithic.Models;
using Microsoft.EntityFrameworkCore;

namespace RTCodingExercise.Monolithic.Services
{
    public class PlateService : IPlateService
    {
        private readonly ApplicationDbContext _context;

        public PlateService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Plate>> GetPlatesForPageAsync(int page, int pageSize, string sortOrder = "asc")
        {
            var query = _context.Plates.AsQueryable();

            query = sortOrder == "desc"
                ? query.OrderByDescending(p => p.SalePrice)
                : query.OrderBy(p => p.SalePrice);

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task AddPlateAsync(Plate plate)
        {
            _context.Plates.Add(plate);
            await _context.SaveChangesAsync();
        }
    }
}
