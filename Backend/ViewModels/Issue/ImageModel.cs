using System;

namespace IssueBase.Issue
{
    public class ImageModel
    {
        public int Id { get; set; }
        public int IssueId { get; set; }
        public DateTime Created { get; set; }

        public ImageInfo Image { get; set; }

        public class ImageInfo
        {
            public ImageInfo(string fileName)
            {
                FileName = fileName;
            }

            public string FileName { get; set; }
        }
    }
}