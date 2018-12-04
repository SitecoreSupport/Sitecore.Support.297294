using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.Mvc;
using System.Web.Routing;
using Sitecore.Diagnostics;
using Sitecore.Services.Infrastructure.Web.Http;

namespace Sitecore.Support.Services.Infrastructure.Web.Http
{

  public class DefaultRouteMapper : IMapRoutes
  {
    public static class RouteName
    {
      public static class ItemService
      {
        public const string Children = "ItemService-Children";

        public const string ContentPath = "ItemService-ContentPath";

        public const string Default = "ItemService";

        public const string Path = "ItemService-Path";

        public const string QueryViaItem = "ItemService-QueryViaItem";

        public const string Search = "ItemService-Search";

        public const string SearchViaItem = "ItemService-SearchViaItem";
      }

      public static class EntityService
      {
        public const string IdAction = "EntityService";

        public const string MetaDataScript = "MetaDataScript";
      }

      public const string Authentication = "Authentication";
    }

    private readonly string _routeBase;

    private const string GuidRegex = "^(\\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\\}{0,1})$";

    public DefaultRouteMapper(string routeBase)
    {
      this._routeBase = (routeBase ?? "");
    }

    public DefaultRouteMapper()
        : this("sitecore/api/ssc/")
    {
    }

    public void MapRoutes(HttpConfiguration config)
    {
      Assert.ArgumentNotNull(config, "config");
      List<IHttpRoute> routes = new List<IHttpRoute>();
      routes.Add(config.Routes.MapHttpRoute("ItemService-QueryViaItem", this._routeBase + "item/{id}/query", new
      {
        controller = "ItemService",
        action = "QueryViaItem",
        @namepsace = "Sitecore-Support-Services-Infrastructure-Sitecore-Controllers"
      }, new
      {
        id = "^(\\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\\}{0,1})$"
      })
        );
      routes.Add(config.Routes.MapHttpRoute("ItemService-Search", this._routeBase + "item/search", new
      {
        controller = "ItemService",
        action = "Search",
        @namepsace = "Sitecore-Support-Services-Infrastructure-Sitecore-Controllers"
      }));
      routes.Add(config.Routes.MapHttpRoute("ItemService-SearchViaItem", this._routeBase + "item/{id}/search", new
      {
        controller = "ItemService",
        action = "SearchViaItem",
        @namepsace = "Sitecore-Support-Services-Infrastructure-Sitecore-Controllers"
      }, new
      {
        id = "^(\\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\\}{0,1})$"
      }));
      routes.Add(config.Routes.MapHttpRoute("ItemService-Children", this._routeBase + "item/{id}/children", new
      {
        controller = "ItemService",
        action = "GetChildren",
        @namepsace = "Sitecore-Support-Services-Infrastructure-Sitecore-Controllers"
      }, new
      {
        id = "^(\\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\\}{0,1})$"
      }));
      config.Routes.MapHttpRoute("ItemService", this._routeBase + "item/{id}", new
      {
        controller = "ItemService",
        action = "DefaultAction",
        @namepsace = "Sitecore-Support-Services-Infrastructure-Sitecore-Controllers"
      }, new
      {
        id = "^(\\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\\}{0,1})$"
      });
      routes.Add(config.Routes.MapHttpRoute("ItemService-ContentPath", this._routeBase + "item", new
      {
        controller = "ItemService",
        action = "GetItemByContentPath",
        @namepsace = "Sitecore-Support-Services-Infrastructure-Sitecore-Controllers"
      }));
      routes.Add(config.Routes.MapHttpRoute("ItemService-Path", this._routeBase + "item/{*path}", new
      {
        controller = "ItemService",
        @namepsace = "Sitecore-Support-Services-Infrastructure-Sitecore-Controllers"
      }));
      foreach (var httpRoute in routes)
      {
        if (httpRoute.Defaults["controller"].Equals("ItemService"))
        {
          httpRoute.Defaults.Add("namespace", "Sitecore-Support-Services-Infrastructure-Sitecore-Controllers");
          Log.Audit("added", new object());
        }
      }


      config.Routes.MapHttpRoute("Authentication", this._routeBase + "auth/{action}", new
      {
        controller = "AuthenticationServiceApi"
      });
      config.Routes.MapHttpRoute("EntityService", this._routeBase + "{namespace}/{controller}/{id}/{action}", new
      {
        id = RouteParameter.Optional,
        action = "DefaultAction"
      });
    }

    public void MapRoutes(RouteCollection routes)
    {
      routes.MapRoute("MetaDataScript", this._routeBase + "script/metadata", new
      {
        controller = "MetaDataScript",
        action = "GetScripts"
      }, new string[1]
      {
            "Sitecore.Services.Infrastructure.Sitecore.Mvc"
      });
    }
  }

}