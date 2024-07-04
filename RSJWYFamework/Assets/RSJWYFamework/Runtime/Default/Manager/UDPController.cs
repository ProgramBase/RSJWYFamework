using System.Net;
using RSJWYFamework.Runtime.Module;
using RSJWYFamework.Runtime.NetWork.Base;
using RSJWYFamework.Runtime.NetWork.UDP;
using RSJWYFamework.Runtime.Utility;

namespace RSJWYFamework.Runtime.Default.Manager
{
    public class UDPController:ISocketUDPController,IModule
    {
        UDPService _udpService;
        
        
        public void Init()
        {
            _udpService = new();
            _udpService.SocketUDPController = this;
        }

        public void Close()
        {
            _udpService?.Close();
            
        }

        public void InitListen(string ip = "any", int port = 5000)
        {
            string lowerip= ip.ToLower();
            //检查是不是监听全部IP
            if (lowerip != "any")
            {
                //指定IP
                //检查IP和Port是否合法
                if (Utility.Utility.SocketTool.MatchIP(ip) && Utility.Utility.SocketTool.MatchPort(port))
                {
                    _udpService.Init(ip, port);
                    return;
                }
            }
            else
            {
                //监听全部IP
                //检查Port是否合法
                if (Utility.Utility.SocketTool.MatchPort(port))
                {
                    _udpService.Init(IPAddress.Any, port);
                    return;
                }
            }
            //全部错误则使用默认参数
            _udpService.Init(IPAddress.Any, 5000);
        }

        public void ReceiveMsgCallBack(string command)
        {
            throw new System.NotImplementedException();
        }
    }
}