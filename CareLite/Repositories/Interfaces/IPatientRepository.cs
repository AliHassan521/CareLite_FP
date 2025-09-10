using CareLite.Models.Domain;

namespace CareLite.Repositories.Interfaces
{
    public interface IPatientRepository
    {
        Task<Patient> CreatePatientAsync(Patient patient);
        Task<List<Patient>> FindPatientDuplicatesAsync(string? fullName, string? email, string? phone);
        Task<(List<Patient> Patients, int TotalCount)> SearchPatientsAsync(string? query, int page, int pageSize);
        Task<Patient> GetPatientByIdAsync(int patientId);
    }
}
