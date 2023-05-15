namespace API.Extensions
{
    public static class DateTimeExtensions
    {
        public static int CalculateAge(this DateOnly birthDate)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var age = today.Year - birthDate.Year;

            if (birthDate > today.AddDays(-age)) age--;

            return age;
        }
    }
}
