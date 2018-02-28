using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using DataTypes;
using Xamarin.Forms;

namespace ASFT.ViewModels
{
    public class LoginViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private readonly ICommand loginCommand = null;

        private string host;

        private string password;

        private string username;

        public LoginViewModel()
        {
            Data = App.Client.GetCurrentLoginModel();
        }

        public LoginViewModel Data { get; }

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
            string errorMsg = "";
            bool bSuccess = await Task.Run(() =>
            {
                try
                {
                    return App.Client.Login(Data.Host, Data.Username, Data.Password);
                }
                catch (InvalidCredentialsException)
                {
                    errorMsg = "Invalid Credentials";
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
                await NavigateBack();
            }
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}