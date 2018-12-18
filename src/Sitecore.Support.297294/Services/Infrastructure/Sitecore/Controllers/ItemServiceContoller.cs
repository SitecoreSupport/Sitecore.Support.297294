using System;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.Routing;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Services.Core;
using Sitecore.Services.Core.Diagnostics;
using Sitecore.Services.Core.Extensions;
using Sitecore.Services.Core.Model;
using Sitecore.Services.Infrastructure.Model;
using Sitecore.Services.Infrastructure.Net.Http;
using Sitecore.Services.Infrastructure.Sitecore;
using Sitecore.Services.Infrastructure.Sitecore.Data;
using Sitecore.Services.Infrastructure.Sitecore.Diagnostics;
using Sitecore.Services.Infrastructure.Sitecore.Handlers;
using Sitecore.Services.Infrastructure.Sitecore.Handlers.Query;
using Sitecore.Services.Infrastructure.Web.Http;
using Sitecore.Services.Infrastructure.Web.Http.Filters;
using Sitecore.Services.Infrastructure.Web.Http.ModelBinding;
using Sitecore.Support.Services.Infrastructure.Sitecore.Data;
using SearchHandler = Sitecore.Support.Services.Infrastructure.Sitecore.Handlers.SearchHandler;
using ItemSearch = Sitecore.Support.Services.Infrastructure.Sitecore.Data.ItemSearch;

namespace Sitecore.Support.Services.Infrastructure.Sitecore.Controllers
{
  [ServicesController]
  [AnonymousUserFilter(AllowAnonymous = AllowAnonymousOptions.UseConfig)]
  public sealed class ItemServiceController : ServicesApiController
  {
    private const int PageSize = 10;

    private readonly IHandlerProvider _handlerProvider;

    private readonly ILogger _logger;


    public ItemServiceController()
    {
      this._handlerProvider = new HandlerProvider();
      this._logger = new SitecoreLogger();
    }

    public ItemServiceController(IHandlerProvider handlerProvider, ILogger logger)
    {
      if (handlerProvider == null)
      {
        throw new ArgumentNullException("handlerProvider");
      }
      if (logger == null)
      {
        throw new ArgumentNullException("logger");
      }
      this._handlerProvider = handlerProvider;
      this._logger = logger;
    }

    [ActionName("GetItemByContentPath")]
    public ItemModel Get([ModelBinder(typeof(GetItemByContentPathQueryModelBinder))] GetItemByContentPathQuery query)
    {
      IItemRequestHandler handler = this._handlerProvider.GetHandler<GetItemByContentPathHandler>();
      IItemRequestHandler itemRequestHandler = handler;
      return (ItemModel)this.ProcessRequest(itemRequestHandler.Handle, query);
    }

    [ActionName("DefaultAction")]
    public ItemModel Get([ModelBinder(typeof(GetItemByIdQueryModelBinder))] GetItemByIdQuery query)
    {
      IItemRequestHandler handler = this._handlerProvider.GetHandler<GetItemByIdHandler>();
      IItemRequestHandler itemRequestHandler = handler;
      return (ItemModel)this.ProcessRequest(itemRequestHandler.Handle, query);
    }

    public ItemModel[] GetChildren([ModelBinder(typeof(GetItemChildrenQueryModelBinder))] GetItemChildrenQuery query)
    {
      IItemRequestHandler handler = this._handlerProvider.GetHandler<GetItemChildrenHandler>();
      IItemRequestHandler itemRequestHandler = handler;
      return (ItemModel[])this.ProcessRequest(itemRequestHandler.Handle, query);
    }

    [HttpGet]
    public HttpResponseMessage QueryViaItem(Guid id, bool includeStandardTemplateFields = false, string fields = "", int page = 0, int pageSize = 10, string database = "", string language = "", string version = "")
    {
      if (pageSize < 1)
      {
        pageSize = 10;
      }
      SitecoreQueryViaItemQuery query = new SitecoreQueryViaItemQuery
      {
        Id = id,
        Database = database,
        Language = language,
        Version = version
      };
      IItemRequestHandler handler = this._handlerProvider.GetHandler<SitecoreQueryViaItemHandler>();
      IItemRequestHandler itemRequestHandler = handler;
      Item[] items = (Item[])this.ProcessRequest(itemRequestHandler.Handle, query);
      FormatItemsQuery query2 = new FormatItemsQuery
      {
        Items = items,
        Fields = fields,
        IncludeStandardTemplateFields = includeStandardTemplateFields,
        Page = page,
        PageSize = pageSize,
        RequestMessage = base.Request,
        Controller = "ItemService-QueryViaItem",
        RouteValues = new
        {
          includeStandardTemplateFields,
          fields,
          database,
          id
        }
      };
      IItemRequestHandler handler2 = this._handlerProvider.GetHandler<FormatItemsHandler>();
      IItemRequestHandler itemRequestHandler2 = handler2;
      object value = this.ProcessRequest(itemRequestHandler2.Handle, query2);
      return base.Request.CreateResponse(HttpStatusCode.OK, value);
    }

    [HttpGet]
    public HttpResponseMessage Search(string term, bool includeStandardTemplateFields = false, string fields = "", int page = 0, int pageSize = 10, string database = "", string language = "", string sorting = "", string facet = "")
    {
      if (pageSize < 1)
      {
        pageSize = 10;
      }
      SearchQuery query = new SearchQuery
      {
        Term = term,
        Database = database,
        Language = language,
        Sorting = sorting,
        Page = page,
        PageSize = pageSize,
        Facet = facet
      };
      IItemRequestHandler handler = new SearchHandler(new ItemSearch());
      IItemRequestHandler itemRequestHandler = handler;
      ItemSearchResults itemSearchResults = (ItemSearchResults)this.ProcessRequest(itemRequestHandler.Handle, query);
      IItemRequestHandler handler2 = this._handlerProvider.GetHandler<FormatItemSearchResultsHandler>();
      FormatItemSearchResultsQuery query2 = new FormatItemSearchResultsQuery
      {
        ItemSearchResults = itemSearchResults,
        Fields = fields,
        IncludeStandardTemplateFields = includeStandardTemplateFields,
        Page = page,
        PageSize = pageSize,
        RequestMessage = base.Request,
        Controller = "ItemService-Search",
        RouteValues = new
        {
          includeStandardTemplateFields,
          fields,
          database,
          sorting,
          term,
          facet
        }
      };
      IItemRequestHandler itemRequestHandler2 = handler2;
      object value = this.ProcessRequest(itemRequestHandler2.Handle, query2);
      return base.Request.CreateResponse(HttpStatusCode.OK, value);
    }

    [HttpGet]
    public HttpResponseMessage SearchViaItem([ModelBinder(typeof(SearchViaItemQueryModelBinder))] SearchViaItemQuery query)
    {
      Assert.ArgumentNotNull(query, "query");
      if (query.PageSize < 1)
      {
        query.PageSize = 10;
      }
      IItemRequestHandler handler = this._handlerProvider.GetHandler<SearchViaItemHandler>();
      IItemRequestHandler itemRequestHandler = handler;
      SearchViaItemQueryResponse searchViaItemQueryResponse = (SearchViaItemQueryResponse)this.ProcessRequest(itemRequestHandler.Handle, query);
      IItemRequestHandler handler2 = this._handlerProvider.GetHandler<FormatItemSearchResultsHandler>();
      FormatItemSearchResultsQuery query2 = new FormatItemSearchResultsQuery
      {
        ItemSearchResults = searchViaItemQueryResponse.ItemSearchResults,
        Fields = searchViaItemQueryResponse.SearchRequest.Fields,
        IncludeStandardTemplateFields = searchViaItemQueryResponse.SearchRequest.IncludeStandardTemplateFields,
        Page = query.Page,
        PageSize = query.PageSize,
        RequestMessage = base.Request,
        Controller = "ItemService-SearchViaItem",
        RouteValues = new
        {
          includeStandardTemplateFields = searchViaItemQueryResponse.SearchRequest.IncludeStandardTemplateFields,
          fields = searchViaItemQueryResponse.SearchRequest.Fields,
          database = searchViaItemQueryResponse.SearchRequest.Database,
          sorting = searchViaItemQueryResponse.SearchRequest.Sorting,
          Term = query.Term,
          facet = searchViaItemQueryResponse.SearchRequest.Facet
        }
      };
      IItemRequestHandler itemRequestHandler2 = handler2;
      object value = this.ProcessRequest(itemRequestHandler2.Handle, query2);
      return base.Request.CreateResponse(HttpStatusCode.OK, value);
    }

    [Obsolete("Use SearchViaItem(SearchViaItemQuery query) method instead.")]
    [NonAction]
    public HttpResponseMessage SearchViaItem([ModelBinder(typeof(SearchViaItemQueryModelBinder))] SearchViaItemQuery query, int page = 0, int pageSize = 10)
    {
      return this.SearchViaItem(query);
    }

    [ActionName("DefaultAction")]
    public HttpResponseMessage Post(string path, [FromBody] ItemModel itemModel, string database = "", string language = "")
    {
      CreateItemCommand query = new CreateItemCommand
      {
        Path = path,
        ItemModel = itemModel,
        Database = database,
        Language = language
      };
      IItemRequestHandler handler = this._handlerProvider.GetHandler<CreateItemHandler>();
      IItemRequestHandler itemRequestHandler = handler;
      CreateItemResponse createItemResponse = (CreateItemResponse)this.ProcessRequest(itemRequestHandler.Handle, query);
      HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Created);
      string value = new UrlHelper(base.Request).Link("ItemService", new
      {
        id = createItemResponse.ItemId,
        Database = createItemResponse.Database,
        Language = createItemResponse.Language
      });
      if (!string.IsNullOrEmpty(value))
      {
        httpResponseMessage.Headers.Add("Location", value);
      }
      return httpResponseMessage;
    }

    [ActionName("DefaultAction")]
    public HttpResponseMessage Delete(Guid id, string database = "", string language = "")
    {
      DeleteItemCommand query = new DeleteItemCommand
      {
        Id = id,
        Database = database,
        Language = language
      };
      IItemRequestHandler handler = this._handlerProvider.GetHandler<DeleteItemHandler>();
      IItemRequestHandler itemRequestHandler = handler;
      this.ProcessRequest(itemRequestHandler.Handle, query);
      return new HttpResponseMessage(HttpStatusCode.NoContent);
    }

    [ActionName("DefaultAction")]
    [HttpPatch]
    public HttpResponseMessage Update(Guid id, [FromBody] ItemModel itemModel, string database = "", string language = "", string version = "")
    {
      UpdateItemCommand query = new UpdateItemCommand
      {
        Id = id,
        ItemModel = itemModel,
        Database = database,
        Language = language,
        Version = version
      };
      IItemRequestHandler handler = this._handlerProvider.GetHandler<UpdateItemHandler>();
      IItemRequestHandler itemRequestHandler = handler;
      this.ProcessRequest(itemRequestHandler.Handle, query);
      return new HttpResponseMessage(HttpStatusCode.NoContent);
    }

    private T ProcessRequest<T>(Func<HandlerRequest, T> handler, HandlerRequest query)
    {
      try
      {
        return handler(query);
      }
      catch (ItemNotFoundException ex)
      {
        throw new ApiControllerException(HttpStatusCode.NotFound, "Item Not Found", ex.Message);
      }
      catch (ArgumentException ex2)
      {
        throw new ApiControllerException(HttpStatusCode.BadRequest, ex2.Message, "");
      }
      catch (ApplicationException ex3)
      {
        throw new ApiControllerException(HttpStatusCode.ServiceUnavailable, ex3.Message, "");
      }
      catch (Exception ex4)
      {
        if (ex4.IsAccessViolation())
        {
          this._logger.Warn(FormattableString.Invariant(FormattableStringFactory.Create("Access Denied: {0}\n\nRequest from {1}", ex4.Message, base.Request.GetClientIpAddress())));
          throw new ApiControllerException(HttpStatusCode.Forbidden);
        }
        this._logger.Error(ex4.ToString());
        throw new ApiControllerException(HttpStatusCode.InternalServerError);
      }
    }
  }

}