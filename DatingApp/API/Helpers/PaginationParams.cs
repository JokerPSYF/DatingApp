namespace API.Helpers
{
    public class PaginationParams
    {
        private const int MaxPageSize = 100;

        public int PageNumber { get; set; } = 1;

        private int pageSize = 12;

        public int PageSize
        {
            get => pageSize;
            set => pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }
}
