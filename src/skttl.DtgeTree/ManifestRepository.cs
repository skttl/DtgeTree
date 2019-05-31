using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using skttl.DtgeTree.Models;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Configuration.Grid;
using Umbraco.Core.Composing;

namespace skttl.DtgeTree
{
    public class ManifestRepository
	{
		private readonly string pluginPrefix = "Dtge";
		private readonly string _defaultManifest;
		private readonly IAppPolicyCache _cache;
		private readonly IGridConfig _gridConfig;

		public ManifestRepository()
		{
            _cache = Current.AppCaches.RuntimeCache;
			_gridConfig = Current.Configs.GetConfig<IGridConfig>();
            _defaultManifest = GetDefaultManifest("{gridEditors:[{\"name\": \"\",\"alias\": \"\",\"view\": \"/App_Plugins/DocTypeGridEditor/Views/doctypegrideditor.html\",\"render\": \"/Views/Partials/TypedGrid/Editors/DocTypeGridEditor.cshtml\",\"icon\": \"icon-document\",\"config\": {\"allowedDocTypes\": [  ], \"nameTemplate\": \"\", \"enablePreview\": true, \"largeDialog\": false, \"showDocTypeSelectAsGrid\": false, \"viewPath\": \"/Views/Partials/TypedGrid/Editors/\", \"previewViewPath\": \"/Views/Partials/TypedGrid/Editors/Previews/\", \"previewCssFilePath\": \"\", \"previewJsFilePath\": \"\" }}]}");
		}

        public string GetDefaultManifest(string fallbackManifest)
        {
            var httpContext = HttpContext.Current;
            if (httpContext == null) return fallbackManifest;

            var defaultManifestPath = httpContext.Server.MapPath("~/App_Plugins/DtgeTree/package.manifest.default");

            if (!File.Exists(defaultManifestPath)) return fallbackManifest;

            return File.ReadAllText(defaultManifestPath);
        }

		public DtgeManifest ScaffoldManifest()
		{
			return GenerateManifest(_defaultManifest);
		}

		public DtgeManifest GenerateManifest(string manifestString)
		{
			var manifestContent = JsonConvert.DeserializeObject<dynamic>(manifestString);

			var gridEditor = manifestContent.gridEditors != null && manifestContent.gridEditors[0] != null ? manifestContent.gridEditors[0] : null;

			if (gridEditor != null)
			{
				try
				{
					var manifest = new DtgeManifest();

					manifest.Name = gridEditor.name;
					manifest.Alias = gridEditor.alias;
					manifest.Icon = gridEditor.icon;
					manifest.AllowedDocTypes = new List<string>();
					foreach (var doctype in gridEditor.config.allowedDocTypes)
					{
						manifest.AllowedDocTypes.Add(doctype.ToString());
					}
					manifest.EnablePreview = gridEditor.config.enablePreview;
                    manifest.LargeDialog = gridEditor.config.largeDialog;
                    manifest.ShowDocTypeSelectAsGrid = gridEditor.config.showDocTypeSelectAsGrid;
                    manifest.ViewPath = gridEditor.config.viewPath != "/Views/Partials/TypedGrid/Editors/" ? gridEditor.config.viewPath : "";
					manifest.PreviewViewPath = gridEditor.config.previewViewPath != "/Views/Partials/TypedGrid/Editors/Previews/" ? gridEditor.config.previewViewPath : "";
					manifest.PackageManifest = manifestString;
					manifest.PackageManifestJson = manifestContent;

					return manifest;
				}
				catch (Exception e)
				{
					return null;
				}
			}
			else
			{
				return null;
			}

		}

		public DtgeManifest GetManifest(string alias)
		{
			var manifests = GetAllCachedManifests();
			return manifests.FirstOrDefault(x => x.Alias == alias);
		}

		public List<DtgeManifest> GetAllCachedManifests(bool purgeCache = false)
		{
			if (purgeCache) _cache.ClearByKey("skttlDtgeTreeManifests");
			return _cache.GetCacheItem<List<DtgeManifest>>("skttlDtgeTreeManifests", () => GetAllManifests(), new TimeSpan(1, 0, 0));
		}

		public List<DtgeManifest> GetAllManifests()
		{
			var manifests = new List<DtgeManifest>();
			var path = HttpContext.Current.Server.MapPath("~/App_Plugins/");
			var directories = Directory.GetDirectories(path);
			var manifestFiles = directories.Where(x => x.Contains("\\" + pluginPrefix + ".") && File.Exists(x + "\\package.manifest")).Select(x => x + "\\package.manifest");
			foreach (var manifestFile in manifestFiles)
			{
				var manifestString = File.ReadAllText(manifestFile);
				var manifest = GenerateManifest(manifestString);
				if (manifest != null)
				{
					manifest.Path = manifestFile;
					manifests.Add(manifest);
				}
				
			}
			
			return manifests;

		}

		public bool IsManifestLoaded(DtgeManifest manifest)
		{
			return _gridConfig.EditorsConfig.Editors.Any(x => x.Alias == manifest.Alias);
		}

		public bool IsManifestLoaded(string alias)
		{
			return _gridConfig.EditorsConfig.Editors.Any(x => x.Alias == alias);
		}

		public void DeleteManifest(string path)
		{
			if (File.Exists(path))
			{
				File.Delete(path);
				Directory.Delete(path.Replace("/package.manifest", ""));
			}
		}


		public void SaveManifest(DtgeManifest manifest)
		{
			dynamic manifestContent;

			if (string.IsNullOrEmpty(manifest.PackageManifest))
			{
				manifestContent = JsonConvert.DeserializeObject<dynamic>(_defaultManifest);
			}
			else
			{
				manifestContent = JsonConvert.DeserializeObject<dynamic>(manifest.PackageManifest);
			}

			var gridEditor = manifestContent.gridEditors != null && manifestContent.gridEditors[0] != null ? manifestContent.gridEditors[0] : null;

			if (gridEditor == null) return;

			gridEditor.name = manifest.Name;
			gridEditor.alias = manifest.Alias;
			gridEditor.icon = manifest.Icon;
			gridEditor.config.allowedDocTypes = new JArray(manifest.AllowedDocTypes.ToArray());
			gridEditor.config.enablePreview = manifest.EnablePreview;
            gridEditor.config.largeDialog = manifest.LargeDialog;
            gridEditor.config.showDocTypeSelectAsGrid = manifest.ShowDocTypeSelectAsGrid;

			if (!string.IsNullOrEmpty(manifest.ViewPath)) gridEditor.config.viewPath = manifest.ViewPath;
			if (!string.IsNullOrEmpty(manifest.PreviewViewPath)) gridEditor.config.previewViewPath = manifest.PreviewViewPath;

			var manifestJson = new JObject();
			var gridEditors = new JArray();
			gridEditors.Add(gridEditor);
			manifestJson["gridEditors"] = gridEditors;

			var dirPath = HttpContext.Current.Server.MapPath("~/App_Plugins/" + pluginPrefix + "." + manifest.Alias);
			var path = dirPath + "/package.manifest";

			if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

			File.WriteAllText(path, JsonConvert.SerializeObject(manifestJson, Formatting.Indented));
			
		}
    }
}
