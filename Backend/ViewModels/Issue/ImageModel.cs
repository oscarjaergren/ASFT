using System;
using Xamarin.Forms;

namespace IssueBase.Issue
{
    public class ImageModel
    {
        public ImageModel()
        {
            ImageId = Guid.NewGuid();
        }
        public ImageModel(ImageModel image)
        {
            ImageId = Guid.NewGuid();
            ImageIssueId = image.ImageIssueId;
        }
        public bool IsImageUpdate = false;
       
        public Guid ImageId { get; set; }
        public ImageSource Source { get; set; }
        public byte[] OrgImage { get; set; }
        public int ImageIssueId { get; set; }
        public string FileName { get; set; }
    }
}
