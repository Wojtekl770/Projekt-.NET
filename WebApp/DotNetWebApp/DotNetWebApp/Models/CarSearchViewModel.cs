namespace DotNetWebApp.Models
{
    public class CarSearchViewModel
    {
        public string? SearchQuery { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public PagedResult<CarOverall> Results { get; set; } = new PagedResult<CarOverall>();

    }
}
