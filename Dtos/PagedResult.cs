using System.Collections.Generic;

namespace PatientsApi.Dtos
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = null!;
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
