using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using IssueBase.Issue;
using Xamarin.Forms;

namespace ASFT.ViewModels
{
    public class ImageViewModel : ImageModel, INotifyPropertyChanged
    {
        private string localImagePath = "";


        public bool IsImageUpdate;

        public ImageViewModel(ImageModel issue)
        {
            ImageId = Guid.NewGuid();
            Id = issue.Id;
            IssueId = issue.IssueId;
            Created = issue.Created;
            Image = new ImageInfo(issue.Image.FileName);
        }

        public ImageViewModel()
        {
            ImageId = Guid.NewGuid();
        }

        public Guid ImageId { get; set; }
        public ImageSource Source { get; set; }
        public byte[] OrgImage { get; set; }

        public string LocalImagePath
        {
            get
            {
                if (localImagePath.Length == 0)
                    return "failedimagesmall.png";

                return localImagePath;
            }
            set
            {
                if (localImagePath != value)
                {
                    localImagePath = value;
                    IsImageUpdate = true;
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            // System.Diagnostics.Debug.WriteLine("Update!"); //ok
            //PropertyChanged is always null and shouldn't.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}