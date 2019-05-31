using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Composing;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Trees;

namespace skttl.DtgeTree
{
    public class DtgeTreeComponent : IComponent
    {
        public void Initialize()
        {
            TreeControllerBase.MenuRendering += TreeControllerBase_MenuRendering;
        }

        void TreeControllerBase_MenuRendering(TreeControllerBase sender, MenuRenderingEventArgs e)
        {
            var treetype = e.QueryStrings.Get("treeType");

            if (e.Menu != null && sender != null && sender.TreeAlias != null && sender.TreeAlias.ToLower() == "documenttypes")
            {
                var menuDocTypeId = -1;
                int.TryParse(e.QueryStrings.Get("id"), out menuDocTypeId);

                if (menuDocTypeId > 0)
                {
                    var label = "Create DTGE";
                    var menuItem = new MenuItem("createDtge", label);
                    menuItem.Icon = "layout";
                    menuItem.NavigateToRoute("/settings/dtges/edit/-1?create=" + menuDocTypeId);
                    e.Menu.Items.Insert(e.Menu.Items.Count, menuItem);
                }
            }
        }

        public void Terminate()
        {
        }
    }
}
