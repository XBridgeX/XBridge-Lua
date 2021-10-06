using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XBridge_Lua.Utils;
using NLua;
using System.Threading;

namespace XBridge_Lua
{
    class Program
    {
        public static Lua lua;
        public delegate Websocket GetWebsocketClient(string url, string name, string pwd);
        public delegate int Schdule(LuaFunction function, int d, int c);
        public delegate bool Cancel(int id);
        public delegate string AesEncrypt(string dt, string k, string iv);
        public delegate string AesDecrypt(string dt, string k, string iv);
        static GetWebsocketClient cs_getWebsocket = (u, n, p) =>
        {
            return new Websocket(u, n, p);
        };
        static Schdule cs_schdule = (f, s, c) =>
        {
            return Utils.Schdule.Schedule(f, s, c);
        };
        static Cancel cs_cancel = (id) =>
        {
            return Utils.Schdule.Cancel(id);
        };
        static AesEncrypt cs_AesEncrypt = (d, k, i) =>
        {
            return AES.AesEncrypt(d, k, i);
        };
        static AesDecrypt cs_AesDecrypt = (d, k, i) =>
        {
            return AES.AesDecrypt(d, k, i);
        };
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine("请指定一个要加载的LUA文件");
                return; 
            }
            lua = new Lua();
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            lua.State.Encoding = Encoding.UTF8;
            lua["AESEncrypt"] = cs_AesEncrypt;
            lua["AESDecrypt"] = cs_AesDecrypt;
            lua["GetWebsocketClient"] = cs_getWebsocket;
            lua["Schedule"] = cs_schdule;
            lua["Cancel"] = cs_cancel;
            lua.LoadCLRPackage();
            try
            {
                lua.DoFile(args[0]);
            }
            catch(Exception e)
            {
                Console.WriteLine($"[{DateTime.Now}][ERROR] " + e.ToString());
            }
            new Thread(() =>
            {
                while (true)
                {
                    string l = Console.ReadLine();
                }
            }).Start();
        }
    }
}
