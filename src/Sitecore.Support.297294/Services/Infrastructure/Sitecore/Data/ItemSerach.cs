using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Sitecore.Configuration;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.ContentSearch.Security;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Services.Infrastructure.Sitecore.Data;
using Sitecore.Services.Infrastructure.Sitecore.Services;

namespace Sitecore.Support.Services.Infrastructure.Sitecore.Data
{
  public class ItemSerach : ItemDataBase, IItemSearch
  {
    private readonly IContentSearchManagerWrapper _contentSearchManagerWrapper;

    private readonly IQueryableOperations _queryableOperations;

    public ItemSerach()
        : this(new ContentSearchManagerWrapper(), new QueryableOperations())
    {
    }

    public ItemSerach(IContentSearchManagerWrapper contentSearchManagerWrapper, IQueryableOperations queryableOperations)
    {
      Assert.ArgumentNotNull(contentSearchManagerWrapper, "_contentSearchManagerWrapper");
      Assert.ArgumentNotNull(queryableOperations, "_queryableOperations");
      this._contentSearchManagerWrapper = contentSearchManagerWrapper;
      this._queryableOperations = queryableOperations;
    }

    public ItemSearchResults Search(string term, string databaseName, string language, string sorting, int page, int pageSize, string facet)
    {
      Assert.ArgumentNotNullOrEmpty(term, "Missing search term");
      string searchIndexNameForDatabase = this.GetSearchIndexNameForDatabase(ItemDataBase.GetDatabaseName(databaseName));
      if (searchIndexNameForDatabase == null)
      {
        throw new ArgumentException(ItemDataBase.InvalidParameterMessage("Database", databaseName));
      }
      ISearchIndex searchIndexFor = this.GetSearchIndexFor(searchIndexNameForDatabase);
      string value = term.ToLower();
      using (IProviderSearchContext context = searchIndexFor.CreateSearchContext(SearchSecurityOptions.Default))
      {
        List<SearchStringModel> list = SearchStringModel.ExtractSearchQuery(term);
        if (list.Count == 0)
        {
          list.Add(new SearchStringModel
          {
            Type = FormattableString.Invariant(FormattableStringFactory.Create("{0}|{1}|{2}", "_content", "_name", "_displayname")),
            Value = value,
            Operation = "must"
          });
        }
        IQueryable<FullTextSearchResultItem> source = this._queryableOperations.CreateQuery(context, list);
        if (Settings.GetBoolSetting("Sitecore.Services.Search.UseDefaultFacets", false))
        {
          source = this._queryableOperations.FacetOn(source, (FullTextSearchResultItem x) => x.Content);
          source = this._queryableOperations.FacetOn(source, (FullTextSearchResultItem x) => x.TemplateName);
          source = this._queryableOperations.FacetOn(source, (FullTextSearchResultItem x) => x.CreatedBy);
          source = this._queryableOperations.FacetOn(source, (FullTextSearchResultItem x) => x.Language);
        }

        if (!string.IsNullOrEmpty(facet))
        {
          string[] facetSegments = facet.Split('|');
          source = this._queryableOperations.Where(source, (FullTextSearchResultItem x) => ((SearchResultItem)x)[facetSegments.First()] == facetSegments.Last());
          foreach (string facetSegment in facetSegments)
          {
            source = this._queryableOperations.FacetOn(source, (FullTextSearchResultItem x) => facetSegment);
          }
          
        }
        if (ItemSerach.IsLanguageSpecificSearch(language))
        {
          string itemLanguage = ItemDataBase.GetLanguage(language).Name;
          source = this._queryableOperations.Where(source, (FullTextSearchResultItem x) => x.Language == itemLanguage);
        }
        source = this._queryableOperations.SortOrder(source, sorting);
        SearchResults<FullTextSearchResultItem> results = this._queryableOperations.GetResults(source);
        source = this._queryableOperations.Skip(source, pageSize * page);
        source = this._queryableOperations.Take(source, pageSize);
        SearchResults<FullTextSearchResultItem> results2 = this._queryableOperations.GetResults(source);
        Item[] items = (from i in this._queryableOperations.HitsSelect(results2, (SearchHit<FullTextSearchResultItem> x) => x.Document.GetItem())
                        where i != null
                        select i).ToArray();
        int num = this._queryableOperations.HitsCount(results);
        return new ItemSearchResults
        {
          TotalCount = num,
          NumberOfPages = ItemSerach.CalculateNumberOfPages(pageSize, num),
          Items = items,
          Facets = this._queryableOperations.Facets(results)
        };
      }
    }

    private ISearchIndex GetSearchIndexFor(string indexName)
    {
      try
      {
        return this._contentSearchManagerWrapper.GetIndex(indexName);
      }
      catch (Exception innerException)
      {
        throw new ApplicationException(FormattableString.Invariant(FormattableStringFactory.Create("Failed to get index ({0})", indexName)), innerException);
      }
    }

    private static bool IsLanguageSpecificSearch(string language)
    {
      return string.Compare(language, "all", StringComparison.OrdinalIgnoreCase) != 0;
    }

    private static int CalculateNumberOfPages(int pageSize, int totalResults)
    {
      return (int)Math.Ceiling((double)totalResults / (double)pageSize);
    }

    public string GetSearchIndexNameForDatabase(string databaseName)
    {
      Assert.ArgumentNotNullOrEmpty(databaseName, "databaseName");
      Dictionary<string, string> dictionary = new Dictionary<string, string>
        {
            {
                "master",
                "sitecore_master_index"
            },
            {
                "web",
                "sitecore_web_index"
            },
            {
                "core",
                "sitecore_core_index"
            }
        };
      string key = databaseName.ToLower();
      if (dictionary.ContainsKey(key))
      {
        return dictionary[key];
      }
      return null;
    }
  }

}