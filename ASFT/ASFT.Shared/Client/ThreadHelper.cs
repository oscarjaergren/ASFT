using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ASFT.IssueManager.Interfaces;
using ASFT.IssueManager.Shared.Client;

#if WINDOWS_PHONE_APP
using Windows.System.Threading;
#endif

[assembly: Xamarin.Forms.Dependency(typeof(ThreadHelper))]
namespace ASFT.IssueManager.Shared.Client
{
  public class ThreadHelper : IThreadHelper
    {
#if __ANDROID__ || __IOS__
      Thread _bkThread;
#endif

#if WINDOWS_PHONE_APP
    
#endif

      public void RunInBackground(Action action)
    {
#if __ANDROID__ || __IOS__
        _bkThread = new Thread(new ThreadStart(action));
        _bkThread.Start();
#endif

#if WINDOWS_PHONE_APP

#endif

    }

    }
}
