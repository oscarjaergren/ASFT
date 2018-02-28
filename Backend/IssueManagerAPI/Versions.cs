using System.Collections.Generic;
using System.Linq;

namespace IssueManagerAPI
{
	public class Version
	{
		public int Number { get; set; }
		public string Name { get; set; }
		public bool IsSupported { get; set; }
	}

	public class Versions
	{
		private static readonly List<Version> AllVersions = new List<Version>
		{
			new Version
			{
				Number = 1,
				Name = "v1",
				IsSupported = true
			}
		};

		public static Version GetLatest()
		{
			return AllVersions.Last();
		}

		public static bool IsVersionSupported( int versionNr )
		{
			var version = AllVersions.FirstOrDefault( x => x.Number == versionNr );
			return version != null && version.IsSupported;
		}
	}
}
