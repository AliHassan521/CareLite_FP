using System;
using CareLite.Services.Interfaces;

namespace CareLite.Services.Implementations
{
    public class BusinessHoursService : IBusinessHoursService
    {
        public (TimeSpan Start, TimeSpan End) GetBusinessHoursForProvider(int providerId)
        {
            // TODO: Replace with real data source (e.g., DB or config)
            // Example: 9 AM to 5 PM
            return (TimeSpan.FromHours(9), TimeSpan.FromHours(17));
        }
    }
}