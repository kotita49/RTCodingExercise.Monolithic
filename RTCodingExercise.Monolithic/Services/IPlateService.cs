using RTCodingExercise.Monolithic.Models;

namespace RTCodingExercise.Monolithic.Services
{
    public interface IPlateService
    {
        Task<List<Plate>> GetPlatesForPageAsync(int page, int pageSize, string sortOrder = "asc");
        Task AddPlateAsync(Plate plate);
    }
}
