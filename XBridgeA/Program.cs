using NLua;
using System;
using System.Text;
using System.Threading;
using XBridgeA.Utils;
using ColoryrSDK;
using System.IO;
using XBridgeA.ColorMirai;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace XBridgeA
{
    class Program
    {
        public static Lua lua;
        static Robot Robot = new Robot();
        static botCfg cfg;
        public delegate Websocket GetWebsocketClient(string url, string name, string pwd);
        public delegate int Schdule(LuaFunction function, int d, int c);
        public delegate bool Cancel(int id);
        public delegate string AesEncrypt(string dt, string k, string iv);
        public delegate string AesDecrypt(string dt, string k, string iv);
        public delegate void Listen(string k, LuaFunction f);
        public delegate void SendGroupMessage(long q, string m);
        public delegate void SendFriendMessage(long q, string m);
        static SendFriendMessage cs_sendFriendMessage = (q, t) =>
        {
            if (Robot.IsConnect == false) return;
            var temp = BuildPack.Build(new SendFriendMessagePack
            {
                qq = cfg.qq,
                id = q,
                message = new()
                {
                    t
                }
            }, 54);
            Robot.AddTask(temp);
        };
        static SendGroupMessage cs_sendGroupMessage = (q, t) =>
        {
            if (Robot.IsConnect == false) return;
            var temp = BuildPack.Build(new SendGroupMessagePack
            {
                qq = cfg.qq,
                id = q,
                message = new()
                {
                    t
                }
            }, 52);
            Robot.AddTask(temp);
        };
        static Listen cs_listen = (k, f) =>
        {
            switch (k)
            {
                case "group":
                    gfunc.Add(f);
                    break;
                case "friend":
                    ffunc.Add(f);
                    break;
            }
        };
        static GetWebsocketClient cs_getWebsocket = (u, n, p) =>
        {
            return new Websocket(u, n, p);
        };
        static Schdule cs_schdule = (f, s, c) =>
        {
            return Utils.Schdule.Schedule(f, s, c);
        };
        /*static Cancel cs_cancel = (id) =>
        {
            return Utils.Schdule.Cancel(id);
        };*/
        static AesEncrypt cs_AesEncrypt = (d, k, i) =>
        {
            return AES.AesEncrypt(d, k, i);
        };
        static AesDecrypt cs_AesDecrypt = (d, k, i) =>
        {
            return AES.AesDecrypt(d, k, i);
        };
        
        static List<LuaFunction> gfunc = new List<LuaFunction>();
        static List<LuaFunction> ffunc = new List<LuaFunction>();
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("请指定一个要加载的LUA文件");
                Console.ReadKey();
                return;
            }
            if(File.Exists("./bot/config.json") == false)
            {
                Directory.CreateDirectory("./bot");
                var p = new botCfg()
                {
                    host = "127.0.0.1",
                    port = 8080,
                    qq = 1145141919,
                    re_connect_time = 20
                };
                File.WriteAllText("./bot/config.json",JsonConvert.SerializeObject(p,Formatting.Indented));
            }
            cfg = JsonConvert.DeserializeObject<botCfg>(File.ReadAllText("./bot/config.json"));
            void Message(byte type, string data)
            {
                switch (type)
                {
                    case 49:
                        var pack = JsonConvert.DeserializeObject<GroupMessageEventPack>(data);
                        foreach(LuaFunction f in gfunc)
                        {
                            try
                            {
                                f.Call(pack);
                            }catch(Exception e) { Console.WriteLine($"[{DateTime.Now}][ERROR] {e.ToString()}"); }
                        }
                        break;
                    case 51:
                        var p = JsonConvert.DeserializeObject<FriendMessageEventPack>(data);
                        foreach (LuaFunction f in gfunc)
                        {
                            try
                            {
                                f.Call(p);
                            }
                            catch (Exception e) { Console.WriteLine($"[{DateTime.Now}][ERROR] {e.ToString()}"); }
                        }
                        break;
                }
            }
            void Log(LogType type, string data)
            {

                Console.WriteLine($"[日志][信息]:{type} {data}");
            }

            void State(StateType type)
            {
                Console.WriteLine($"[日志][状态]:{type}");
            }
            var colorcfg = new RobotConfig()
            {
                IP = cfg.host,
                Port = cfg.port,
                Name = "XBridgeA",
                Pack = new() { 49, 51 },
                Time = cfg.re_connect_time,
                CallAction = Message,
                LogAction = Log,
                StateAction = State
            };
            Robot.Set(colorcfg);
            Robot.Start();
            while (!Robot.IsConnect);
            lua = new Lua();
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            lua.State.Encoding = Encoding.UTF8;
            lua["AESEncrypt"] = cs_AesEncrypt;
            lua["AESDecrypt"] = cs_AesDecrypt;
            lua["GetWebsocketClient"] = cs_getWebsocket;
            lua["Schedule"] = cs_schdule;
            lua["Listen"] = cs_listen;
            lua["sendGroupMessage"] = cs_sendGroupMessage;
            lua["sendFriendMessage"] = cs_sendFriendMessage;
            //lua["Cancel"] = cs_cancel;
            lua.LoadCLRPackage();
            try
            {
                lua.DoFile(args[0]);
            }
            catch (Exception e)
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