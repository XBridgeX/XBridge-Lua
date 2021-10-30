using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XBridgeA.Utils
{
    public class Schdule
    {
        private static Dictionary<int, Thread> thr = new Dictionary<int, Thread>();
        public static int Schedule(LuaFunction func, int delay, int cycle)
        {
            var t = new Thread(() =>
            {
                for (int i = 0; i < cycle; i++)
                {                
                    Thread.Sleep(delay*1000);
                    try
                    {
                        func.Call();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"[{DateTime.Now}][ERROR] " + e.ToString());
                    }
                }
            });
            t.Start();
            int id = t.ManagedThreadId;
            thr.Add(id, t);
            new Thread(() =>
            {
                t.Join();
                if (thr.ContainsKey(id))
                    thr.Remove(id);
            }).Start();
            return id;
        }
        /*
        public static bool Cancel(int id)
        {
            if (!thr.ContainsKey(id))
                return false;
            thr[id].;
            thr.Remove(id);
            return true;
        }
        */
    }
}
