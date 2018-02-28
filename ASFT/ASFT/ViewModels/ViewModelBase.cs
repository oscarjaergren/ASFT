using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace ASFT.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        private const string ExecutecommandSuffix = "_ExecuteCommand";
        private const string CanexecutecommandSuffix = "_CanExecuteCommand";
        private readonly Dictionary<string, ICommand> commands;
        public Func<Task> OnBackNavigationRequest;
        public Func<Task> OnCloseNavigationRequest;
        public Func<ViewModelBase, Task> OnModalNavigationRequest;

        public Func<ViewModelBase, Task> OnNavigationRequest;

        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();

        public ViewModelBase()
        {
            commands =
                GetType().GetTypeInfo().DeclaredMethods
                    .Where(dm => dm.Name.EndsWith(ExecutecommandSuffix, StringComparison.Ordinal))
                    .ToDictionary(GetCommandName, GetCommand);
        }

        public ICommand this[string name]
        {
            get
            {
                ICommand cmd = commands[name];
                return cmd;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetValue<T>(T value, [CallerMemberName] string propertyName = null)
        {
            if (!properties.ContainsKey(propertyName)) properties.Add(propertyName, default(T));

            T oldValue = GetValue<T>(propertyName);
            if (!EqualityComparer<T>.Default.Equals(oldValue, value))
            {
                properties[propertyName] = value;
                OnPropertyChanged(propertyName);
            }
        }

        protected T GetValue<T>([CallerMemberName] string propertyName = null)
        {
            if (!properties.ContainsKey(propertyName))
                return default(T);
            return (T) properties[propertyName];
        }

        private string GetCommandName(MethodInfo mi)
        {
            return mi.Name.Replace(ExecutecommandSuffix, "");
        }

        private ICommand GetCommand(MethodInfo mi)
        {
            MethodInfo canExecute =
                GetType().GetTypeInfo().GetDeclaredMethod(GetCommandName(mi) + CanexecutecommandSuffix);
            var executeAction = (Action<object>) mi.CreateDelegate(typeof(Action<object>), this);
            var canExecuteAction = canExecute != null
                ? (Func<object, bool>) canExecute.CreateDelegate(typeof(Func<object, bool>), this)
                : state => true;
            return new Command(executeAction, canExecuteAction);
        }

        public async Task NavigateTo<TViewModel>(TViewModel targetViewModel) where TViewModel : ViewModelBase
        {
            if (OnNavigationRequest != null) await OnNavigationRequest?.Invoke(targetViewModel);
        }

        public async Task NavigateToModal<TViewModel>(TViewModel targetViewModel) where TViewModel : ViewModelBase
        {
            if (OnModalNavigationRequest != null) await OnModalNavigationRequest?.Invoke(targetViewModel);
        }

        public async Task NavigateBack()
        {
            if (OnBackNavigationRequest != null) await OnBackNavigationRequest?.Invoke();
        }

        public async Task Close()
        {
            if (OnCloseNavigationRequest != null) await OnCloseNavigationRequest?.Invoke();
        }
    }
}