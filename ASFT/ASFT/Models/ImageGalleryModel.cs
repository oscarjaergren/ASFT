using System;
using Xamarin.Forms;

namespace ASFT.Models
{
    public class ImageGalleryModel
    {
        public ImageGalleryModel()
        {
            ImageId = Guid.NewGuid();
        }

        public Guid ImageId { get; set; }

        public ImageSource Source { get; set; }
        public byte[] OrgImage { get; set; }
    }
}