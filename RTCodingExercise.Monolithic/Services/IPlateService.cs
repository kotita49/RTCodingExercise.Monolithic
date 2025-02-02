using RTCodingExercise.Monolithic.Models;

namespace RTCodingExercise.Monolithic.Services
{
    public interface IPlateService
    {
        Task<List<Plate>> GetPlatesForPageAsync(int page, int pageSize, string sortOrder = "asc", string? filter = null);
        Task AddPlateAsync(Plate plate);
        Task<Plate> GetPlateByIdAsync(Guid plateId);
        Task SetPlateReservationStatusAsync(Guid plateId, bool isReserved);
    }
}
