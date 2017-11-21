using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DASN.PortableWebApp.Services
{
    public class AntibotService
    {
        private readonly int _lapse;
        public AntibotService(int seconds)
        {
            _lapse = seconds*1000;
        }

        public async Task<ActionResult> SecureActionAsync(Func<Task<ActionResult>> action)
        {
            var watch = new Stopwatch();
            watch.Start();
            var result = await action.Invoke();
            watch.Stop();
            await Task.Delay(TimeSpan.FromMilliseconds(_lapse - watch.ElapsedMilliseconds));
            return result;
        }
    }
}