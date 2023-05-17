namespace API.Extensions
{
    public static class DateTimeExtensions
    {
        public static int CalculateAge(this DateOnly birthDate)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

            int age = today.Year - birthDate.Year;

            if (birthDate > today.AddDays(-age)) age--;

            return age;
        }
    }
}
