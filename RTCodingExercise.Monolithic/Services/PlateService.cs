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

        public async Task<List<Plate>> GetPlatesForPageAsync(int page, int pageSize, string sortOrder = "asc", string? filter = null)
        {
            var query = _context.Plates.AsQueryable();

            // Filter by registration 
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(p => p.Registration.Contains(filter));
            }

            // Filter out reserved plates
            query = query.Where(p => !p.Reserved);

            // Sort by price
            query = sortOrder == "desc"
                ? query.OrderByDescending(p => p.SalePrice)
                : query.OrderBy(p => p.SalePrice);

            // Pagination
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

        public async Task<Plate> GetPlateByIdAsync(Guid plateId)
        {
            return await _context.Plates.FindAsync(plateId);
        }

        // Method to mark plate as reserved or unreserved
        public async Task SetPlateReservationStatusAsync(Guid plateId, bool isReserved)
        {
            var plate = await _context.Plates.FindAsync(plateId);
            if (plate == null) throw new KeyNotFoundException("Plate not found");

            plate.Reserved = isReserved;
            await _context.SaveChangesAsync();            
        }
    }
}
