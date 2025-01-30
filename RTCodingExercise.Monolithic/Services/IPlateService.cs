using RTCodingExercise.Monolithic.Models;

namespace RTCodingExercise.Monolithic.Services
{
    public interface IPlateService
    {
        Task<List<Plate>> GetPlatesForPageAsync(int page, int pageSize);
        Task AddPlateAsync(Plate plate);
    }
}
