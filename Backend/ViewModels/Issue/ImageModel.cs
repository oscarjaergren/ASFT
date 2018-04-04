using System;
using Xamarin.Forms;

namespace IssueBase.Issue
{
    using Newtonsoft.Json;

    public class ImageModel
    {
        public class ImageInfo
        {
            public string FileName { get; set; }

            public ImageInfo(string fileName)
            {
                FileName = fileName;
            }
        }

        public ImageModel()
        {
            ImageId = Guid.NewGuid();
            var FileName = ImageId.ToString();
            Image = new ImageInfo(FileName);
        }

        public ImageModel(ImageModel image)
        {
            ImageIssueId = image.ImageIssueId;
            IssueId = image.IssueId;
            Created = image.Created;
            Image = new ImageInfo(image.Image.FileName);
            ImageId = Guid.NewGuid();
            ImageIssueId = image.ImageIssueId;
        }

        public Guid ImageId { get; set; }

        public ImageSource Source { get; set; }

        public byte[] OrgImage { get; set; }

        [JsonProperty(PropertyName = "Id")]
        public int ImageIssueId { get; set; }

        public int IssueId { get; set; }

        public DateTime Created { get; set; }

        public ImageInfo Image { get; set; }
    }
}

