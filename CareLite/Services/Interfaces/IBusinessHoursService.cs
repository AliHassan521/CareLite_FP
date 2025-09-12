namespace CareLite.Services.Interfaces
{
    public interface IBusinessHoursService
    {
        (TimeSpan Start, TimeSpan End) GetBusinessHoursForProvider(int providerId);
    }
}