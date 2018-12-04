using System;
using Sitecore.Diagnostics;
using Sitecore.Services.Infrastructure.Model;
using Sitecore.Services.Infrastructure.Sitecore.Data;
using Sitecore.Services.Infrastructure.Sitecore.Handlers;

namespace Sitecore.Support.Services.Infrastructure.Sitecore.Handlers
{
  public class SearchHandler : ItemRequestHandler<SearchQuery>
  {
    private readonly IItemSearch _itemSearch;

    public SearchHandler(IItemSearch itemSearch)
    {
      this._itemSearch = itemSearch;
    }

    protected override object HandleRequest(SearchQuery request)
    {
      Assert.ArgumentNotNull(request, "request");
      if (string.IsNullOrEmpty(request.Term))
      {
        throw new ArgumentException("Missing search term");
      }
      return this._itemSearch.Search(request.Term, request.Database, request.Language, request.Sorting, request.Page, request.PageSize, request.Facet);
    }
  }

}