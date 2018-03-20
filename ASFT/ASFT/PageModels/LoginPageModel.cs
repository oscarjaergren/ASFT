namespace ASFT.PageModels
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using ASFT.Models;

    using FreshMvvm;

    using IssueManagerApiClient;

    using Xamarin.Forms;

    public class LoginPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        private readonly ICommand loginCommand = null;

        private string host;

        private string password;

        private string username;

        public LoginPageModel()
        {
            Data = App.Client.GetCurrentLoginModel();
            Username = Data.Username;
            Password = Data.Password;
            Host = Data.Host;
        }

        public LoginModel Data { get; }

        public ICommand LoginCommand
        {
            get { return loginCommand ?? new Command(DoLoginAsync); }
        }

        public string Username
        {
            get { return username; }
            set
            {
                username = value;
                RaisePropertyChanged("Username");
            }
        }

        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                RaisePropertyChanged("Password");
            }
        }

        public string Host
        {
            get { return host; }
            set
            {
                host = value;
                RaisePropertyChanged("Host");
            }
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        private async void DoLoginAsync()
        {
            string errorMsg = string.Empty;
            bool bSuccess = await Task.Run(() =>
            {
                try
                {
                    return App.Client.Login(Data.Host, Data.Username, Data.Password);
                }
                catch (InvalidCredentialsException)
                {
                    errorMsg = "Invalid Credentials";
                    Debug.WriteLine(errorMsg);
                    return false;
                }
                catch (Exception ex)
                {
                    errorMsg = ex.Message;
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                    Exception innerException = ex.InnerException;

                    while (innerException != null)
                    {
                        Debug.WriteLine(innerException.Message);
                        innerException = innerException.InnerException;
                    }

                    return false;
                }
            });

            if (bSuccess)
            {
                await CoreMethods.PopPageModel();
            }
        }

        protected new void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}