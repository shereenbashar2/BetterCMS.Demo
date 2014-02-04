﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using BetterCms.Demo.Web.Models;
using BetterCms.Module.Api;
using BetterCms.Module.Api.Operations.Pages.Sitemap;
using BetterCms.Module.Api.Operations.Pages.Sitemap.Nodes;

namespace BetterCms.Demo.Web.Controllers
{
    public class SiteMapController : Controller
    {
        public virtual ActionResult Index()
        {
            List<MenuItemViewModel> menuItems;

            using (var api = ApiFactory.Create())
            {
                menuItems = GetMenuItems(api, new Guid("17ABFEE9-5AE6-470C-92E1-C2905036574B"));
            }

            return View(menuItems);
        }

        public virtual ActionResult SubMenu(string parentUrl)
        {
            IList<MenuItemViewModel> menuItems = null;

            using (var api = ApiFactory.Create())
            {
                var parentRequest = new GetSitemapNodesRequest();
                parentRequest.Data.Take = 1;
                parentRequest.Data.Filter.Add("ParentId", null);
                parentRequest.Data.Filter.Add("Url", parentUrl);
                parentRequest.Data.Order.Add("DisplayOrder");
                
                var parentResponse = api.Pages.Sitemap.Nodes.Get(parentRequest);
                if (parentResponse.Data.Items.Count == 1)
                {
                    var request = new GetSitemapNodesRequest();
                    request.Data.Filter.Add("ParentId", parentResponse.Data.Items[0].Id);
                    request.Data.Order.Add("DisplayOrder");
                    
                    var response = api.Pages.Sitemap.Nodes.Get(request);
                    if (response.Data.Items.Count > 0)
                    {
                        menuItems = response.Data.Items
                            .Select(mi => new MenuItemViewModel { Caption = mi.Title, Url = mi.Url })
                            .ToList();

                        menuItems.Insert(0, new MenuItemViewModel { Caption = "Main", Url = parentUrl } );
                    }
                }
            }

            return View(menuItems);            
        }

        private List<MenuItemViewModel> GetMenuItems(IApiFacade api, Guid sitemapId)
        {
            var request = new GetSitemapNodesRequest
            {
                SitemapId = new Guid("17ABFEE9-5AE6-470C-92E1-C2905036574B")
            };

            request.Data.Filter.Add("ParentId", null);
            request.Data.Order.Add("DisplayOrder");

            var response = api.Pages.Sitemap.Nodes.Get(request);
            if (response.Data.Items.Count > 0)
            {
                return response.Data.Items.Select(mi => new MenuItemViewModel { Caption = mi.Title, Url = mi.Url }).ToList();
            }

            var allSitemaps = api.Pages.Sitemap.Get(new GetSitemapsRequest());
            if (allSitemaps.Data.Items.Count > 0)
            {
                var sitemap = allSitemaps.Data.Items.First();
                return GetMenuItems(api, sitemap.Id);
            }

            return new List<MenuItemViewModel>();
        }
    }
}
