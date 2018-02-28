using System;
using System.Linq;
using System.Reflection;
using ASFT.ViewModels;
using Xamarin.Forms;

namespace ASFT.HelperMethods
{
    internal static class ViewResolver
    {
        public static Page GetViewFor<TArgetViewModel>(TArgetViewModel targetViewModel)
            where TArgetViewModel : ViewModelBase, new()
        {
            string targetViewName = targetViewModel.GetType().Name.Replace("ViewModel", "Page");
            var definedTypes = targetViewModel.GetType().GetTypeInfo().Assembly.DefinedTypes;
            TypeInfo targetType = definedTypes.FirstOrDefault(t => t.Name == targetViewName);
            return Activator.CreateInstance(targetType.AsType()) as Page;
        }
    }
}