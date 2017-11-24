using skttl.DtgeTree.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace skttl.DtgeTree
{
	[PluginController("DtgeTree")]
    public class ManifestController : UmbracoAuthorizedApiController
    {
		private ManifestRepository _manifestRepository;

		public ManifestController()
		{
			_manifestRepository = new ManifestRepository();
		}

		[HttpPost]
		public void SaveManifest(DtgeManifest manifest)
		{
			_manifestRepository.SaveManifest(manifest);
		}
        
		[HttpGet]
		public DtgeManifest GetManifest(string alias)
		{
			return _manifestRepository.GetManifest(alias);
		}

		[HttpGet]
		public DtgeManifest ScaffoldManifest()
		{
			return _manifestRepository.ScaffoldManifest();
		}

    }
}
