using System;

namespace DataAccess.Models
{
	public class IssueImageModel
	{
		public class ImageInfo
		{
			public string FileName { get; set; }

			public ImageInfo( string fileName )
			{
				FileName = fileName;
			}
		}

		public int Id { get; set; }
		public int IssueId { get; set; }
		public DateTime Created { get; set; }

		public ImageInfo Image { get; set; }
	}
}
