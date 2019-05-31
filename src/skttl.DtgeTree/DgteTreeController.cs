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
	[Tree(Constants.Applications.Settings, "dtges", TreeTitle =  "Doc Type Grid Editors", TreeGroup = Constants.Trees.Groups.Settings)]
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
				add.NavigateToRoute("/settings/dtges/edit/-1?create");
			
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
}
