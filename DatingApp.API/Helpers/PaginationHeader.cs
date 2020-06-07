namespace DatingApp.API.Helpers
{
    public class PaginationHeader
    {
        public int PageNumber { get; set; }

        public int TotalItems { get; set; }

        public int TotalPages { get; set; }

        public int ItemsPerPage { get; set; }
        
        public PaginationHeader(int pageNumber, int totalItems, int totalPages, int itemsPerPage)
        {
            PageNumber = pageNumber;
            TotalItems = totalItems;
            TotalPages = totalPages;
            ItemsPerPage = itemsPerPage;
        }
    }
}