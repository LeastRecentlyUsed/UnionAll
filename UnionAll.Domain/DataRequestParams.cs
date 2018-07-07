namespace UnionAll.Domain
{
    public class DataRequestParams
    {
        private const int _maxPageSize = 1000;
        public int PageNumber { get; set; } = 1;

        private int _pageSize = 100;
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = (value > _maxPageSize) ? _maxPageSize : value; }
        }

        public string SearchQuery { get; set; }
    }
}
