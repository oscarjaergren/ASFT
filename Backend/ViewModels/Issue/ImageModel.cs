using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace IssueBase.Issue
{
    public class ImageModel
    {
        public ImageModel()
        {
            ImageId = Guid.NewGuid();
        }
        public ImageModel(ImageModel issue)
        {
            ImageId = Guid.NewGuid();
            Id = issue.Id;
            IssueId = issue.IssueId;
            Created = issue.Created;
        }
        private string localImagePath = "";
        public bool IsImageUpdate = false;

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
                }
            }
        }
        public Guid ImageId { get; set; }
        public ImageSource Source { get; set; }
        public byte[] OrgImage { get; set; }
        public int Id { get; set; }
        public int IssueId { get; set; }
        public DateTime Created { get; set; }
        public string FileName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            // System.Diagnostics.Debug.WriteLine("Update!"); //ok
            //PropertyChanged is always null and shouldn't.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
