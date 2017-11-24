using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skttl.DtgeTree.Models
{
    public class DtgeManifest
    {
		public string Name { get; set; }
		public string Alias { get; set; }
		public string Icon { get; set; }
		public List<string> AllowedDocTypes { get; set; }
		public bool EnablePreview { get; set; }
		public string ViewPath { get; set; }
		public string PreviewViewPath { get; set; }
		public string PackageManifest { get; set; }
		public dynamic PackageManifestJson { get; set; }
		public string Path { get; set; }
	}
}
