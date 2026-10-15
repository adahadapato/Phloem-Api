
namespace Phloem.Domain.Shared;

public class Pagination
{
    const int maxPageSize = 10000;
    public int PageNumber { get; set; } = 1;
    private int _pageSize = 100;
    public int PageSize
    {
        get
        {
            return _pageSize;
        }
        set
        {
            _pageSize = (value > maxPageSize) ? maxPageSize : value;
        }
    }
}
