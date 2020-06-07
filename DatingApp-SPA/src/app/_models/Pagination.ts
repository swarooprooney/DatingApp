export interface Pagination {
  totalItems: number;
  itemsPerPage: number;
  pageNumber: number;
  totalPages: number;
}

export class PaginatedResult<T> {
    result: T;
    pagination: Pagination;
}
