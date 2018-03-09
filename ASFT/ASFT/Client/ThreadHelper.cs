using System;
using ASFT.Client;
using ASFT.IServices;

#if WINDOWS_PHONE_APP
using Windows.System.Threading;
#endif

[assembly: Xamarin.Forms.Dependency(typeof(ThreadHelper))]
namespace ASFT.Client
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
