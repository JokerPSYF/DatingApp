namespace API.Helpers
{
    public class UserParams
    {
        private const int MaxPageSize = 100;

        public int PageNumber { get; set; } = 1;

        private int pageSize = 18;

        public int PageSize
        {
            get => pageSize;
            set => pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }
}
