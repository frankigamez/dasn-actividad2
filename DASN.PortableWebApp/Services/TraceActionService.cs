using System;
using System.Diagnostics;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DASN.PortableWebApp.Services
{
    public static class TraceActionService
    {
        public static void TraceVoid(this ILog logger, Action actionToTrace,
            [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
        {
            logger.Debug($"IN {caller}");
            var counter = new Stopwatch();
            try
            {
                counter.Start();
                actionToTrace?.Invoke();
            }
            catch (Exception e)
            {
                logger.Error($"ERROR {caller} | {e}");
                throw e;
            }
            finally
            {
                counter.Stop();
                logger.Debug($"OUT {caller}. {counter.ElapsedTicks} ticks ({counter.ElapsedMilliseconds} ms).");
            }
        }

        public static ActionResult TraceActionResult(this ILog logger, Func<ActionResult> actionToTrace,
            [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
        {
            ActionResult result;

            logger.Debug($"IN {caller}");
            var counter = new Stopwatch();
            try
            {
                counter.Start();
                result = actionToTrace?.Invoke();
                logger.Debug($"RESULT {result}");
            }
            catch (Exception e)
            {
                logger.Debug($"ERROR {caller} | {e}");
                throw e;
            }
            finally
            {
                counter.Stop();
                logger.Debug($"OUT {caller}. {counter.ElapsedTicks} ticks ({counter.ElapsedMilliseconds} ms).");
            }            
            
            return result;
        }
        

        public static void TraceEntry(this ILog logger, string name, object value,
            [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
        {
            logger.Debug($"ENTRY {caller} {name} = {JsonConvert.SerializeObject(value)}");
        }
    }
}