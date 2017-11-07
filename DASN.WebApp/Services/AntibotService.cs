using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DASN.WebApp.Services
{
    public class AntibotService
    {
        private readonly int _lapse;
        public AntibotService(int seconds)
        {
            _lapse = seconds*1000;
        }

        public ActionResult SecureAction(Func<ActionResult> action)
        {
            var watch = new Stopwatch();
            watch.Start();
            var result = action.Invoke();
            watch.Stop();
            Task.Delay(TimeSpan.FromMilliseconds(_lapse - watch.ElapsedMilliseconds));
            return result;
        }
    }
}