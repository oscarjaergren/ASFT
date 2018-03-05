using System;
using System.Collections.Generic;
using System.Text;

namespace ASFT.HelperMethods
{
    public class LoadingEventArgs : EventArgs
    {
        public bool IsLoading { get; set; } = false;
    }
}
