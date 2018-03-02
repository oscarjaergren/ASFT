using System;
using FreshMvvm;

namespace ASFT.HelperMethods
{
    public class FreshPageModelMapperreplacement : IFreshPageModelMapper
    {
        public string GetPageTypeName(Type pageModelType)
        {
            return pageModelType.AssemblyQualifiedName
                .Replace("PageModel", "Page")
                .Replace("ViewModel", "Page")
                .Replace("ViewModel", "View");
        }
    }
}

 
