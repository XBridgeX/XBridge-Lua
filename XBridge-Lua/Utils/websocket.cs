using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using NLua;

namespace XBridge_Lua.Utils
{
    public class Websocket
    {
        private WebSocket webSocket;
        private string name;
        private string k;
        private string iv;
        public Websocket(string url, string n, string password)
        {
            webSocket = new WebSocket(url);
            this.name = n;
            k = MD5.MD5Encrypt(password).Substring(0, 16);
            iv = MD5.MD5Encrypt(password).Substring(16);
        }
        public bool Start()
        {
            if (!webSocket.IsAlive)
            {
                webSocket.Connect();
                return true;
            }
            return false;
        }
        public bool IfAlive
        {
            get
            {
                return webSocket.IsAlive;
            }
        }
        public bool Close()
        {
            if (webSocket.IsAlive)
            {
                webSocket.Close();
                return true;
            }
            return false;
        }
        public string getK { get { return this.k; } }
        public string getiv { get { return this.iv; } }
        public bool Send(object message)
        {
            if (webSocket.IsAlive)
            {
                webSocket.Send(message.ToString());
                return true;
            }
            return false;
        }
        /// <summary>
        /// 添加方法处理事件
        /// </summary>
        /// <param name="type">模式</param>
        /// <param name="func">调用函数</param>
        public void AddFunction(string type, LuaFunction func)
        {
            switch (type)
            {
                case "onMessage":
                    webSocket.OnMessage += (sender, message) =>
                    {
                        try
                        {
                            func.Call(name, message.Data);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"[{DateTime.Now}][ERROR] "+e.ToString());
                        }
                    };
                    break;
                case "onOpen":
                    webSocket.OnOpen += (sender, ex) =>
                    {
                        try
                        {
                            func.Call(name);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"[{DateTime.Now}][ERROR] " + e.ToString());
                        }
                    };
                    break;
                case "onClose":
                    webSocket.OnClose += (sender, message) =>
                    {
                        try
                        {
                            func.Call(name, message.Reason);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"[{DateTime.Now}][ERROR] " + e.ToString());
                        }
                    };
                    break;
                case "onError":
                    webSocket.OnError += (sender, message) =>
                    {
                        try
                        {
                            func.Call(name, message.Message);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"[{DateTime.Now}][ERROR] " + e.ToString());
                        }
                    };
                    break;
            }

        }
    }
}
