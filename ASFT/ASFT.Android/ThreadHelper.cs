using System;
using ASFT.Droid;
using ASFT.IServices;

[assembly: Xamarin.Forms.Dependency(typeof(ThreadHelper))]

namespace ASFT.Droid
{
    using System.Threading;

    public class ThreadHelper : IThreadHelper
    {
        public void RunInBackground(Action action)
        {
            Thread backgroundThread = new Thread(new ThreadStart(action));
            backgroundThread.Start();
        }
    }
}