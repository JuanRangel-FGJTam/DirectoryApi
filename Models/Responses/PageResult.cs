using System;

namespace AuthApi.Models.Responses;

public class PagedResponse<T>
{
    public IEnumerable<T> Items {get;set;} = default!;
    public int TotalItems {get;set;}
    public int PageSize {get;set;}
    public int PageNumber {get;set;}
}
