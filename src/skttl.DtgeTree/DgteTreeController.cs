using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Grid;
using Umbraco.Core.IO;
using Umbraco.Web;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;

namespace skttl.DtgeTree
{
	[Tree("developer", "dtges", "Doc Type Grid Editors", "icon-item-arrangement", "icon-item-arrangement")]
	[PluginController("DtgeTree")]
	public class DgteTreeController : TreeController
	{

		private ManifestRepository _manifestRepository { get; set; }

		public DgteTreeController()
		{
			_manifestRepository = new ManifestRepository();
		}

		protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
		{
			var items = new MenuItemCollection();
			

			if (id == "-1")
			{
				var add = new MenuItem("add", "Add new grid editor");
				add.Icon = "page-add";
				add.NavigateToRoute("/developer/dtges/edit/-1?create");
			
				items.Items.Add(add);
			}
			else {
				var delete = new MenuItem("delete", "Delete");
				delete.Icon = "trash";

				items.Items.Add(delete);
			}

			return items;
		}

		protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
		{
			var nodes = new TreeNodeCollection();
			
			if (id == "-1")
			{
				var manifests = _manifestRepository.GetAllCachedManifests(true);
				foreach (var manifest in manifests)
				{
					var treeNode = CreateTreeNode(manifest.Alias, id, queryStrings, manifest.Name, manifest.Icon);

					if (!_manifestRepository.IsManifestLoaded(manifest))
					{
						treeNode.SetNotPublishedStyle();
					}

					nodes.Add(treeNode);
				}
			}

			return nodes;
		}
	}


	public class TreeAction : ApplicationEventHandler
	{
		protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
		{
			TreeControllerBase.MenuRendering += TreeControllerBase_MenuRendering;
		}

		void TreeControllerBase_MenuRendering(TreeControllerBase sender, MenuRenderingEventArgs e)
		{
			var textService = sender.ApplicationContext.Services.TextService;
			var treetype = e.QueryStrings.Get("treeType");

			if (e.Menu != null && sender.TreeAlias.ToLower() == "documenttypes")
			{
				var menuDocTypeId = -1;
				int.TryParse(e.QueryStrings.Get("id"), out menuDocTypeId);

				if (menuDocTypeId > 0)
				{
					var label = "Create DTGE";
					var menuItem = new MenuItem("createDtge", label);
					menuItem.Icon = "layout";
					menuItem.NavigateToRoute("/developer/dtges/edit/-1?create=" + menuDocTypeId);
					e.Menu.Items.Insert(999, menuItem);
				}
			}
		}
	}
}
