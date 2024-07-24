using System;
using RSJWYFamework.Runtime.Default.EventsLibrary;
using RSJWYFamework.Runtime.Event;
using RSJWYFamework.Runtime.Module;
using RSJWYFamework.Runtime.Net.Public;
using RSJWYFamework.Runtime.NetWork.Event;
using RSJWYFamework.Runtime.NetWork.TCP.Client;
using RSJWYFamework.Runtime.Socket.Base;
using RSJWYFamework.Runtime.NetWork.TCP.Server;
using RSJWYFamework.Runtime.Utility;

namespace RSJWYFamework.Runtime.Default.Manager
{
    /// <summary>
    /// 客户端的控制器
    /// </summary>
    public class DefaultTcpClientController : ISocketTCPClientController,IModule
    {
        private TcpClientService tcpsocket;
        
        public void Init()
        {
            Main.Main.EventModle.BindEvent<ServerToClientMsgEventArgs>(ClientSendToServerMsg);
            tcpsocket = new();
            tcpsocket.SocketTcpClientController = this;
        }

        public void Close()
        {
            Main.Main.EventModle.UnBindEvent<ServerToClientMsgEventArgs>(ClientSendToServerMsg);
            tcpsocket?.Quit();
        }

        public void Update(float time, float deltaTime)
        {
            
        }

        public void UpdatePerSecond(float time)
        {
        }


        public void InitTCPClient(string ip = "127.0.0.1", int port = 6000)
        {
            //客户端的
            if (ip != "127.0.0.1")
            {
                //指定目标IP
                //检查IP和Port是否合法
                if (Utility.Utility.SocketTool.MatchIP(ip) && Utility.Utility.SocketTool.MatchPort(port))
                {
                    tcpsocket.Connect(ip, port);
                    tcpsocket.AsyncCheckNetThread().Forget();
                    return;
                }
            }
            else
            {
                //使用默认IP
                //检查Port是否合法
                if (Utility.Utility.SocketTool.MatchPort(port))
                {
                    tcpsocket.Connect("127.0.0.1", port);
                    tcpsocket.AsyncCheckNetThread().Forget();
                    return;
                }
            }

            //全部匹配失败，使用默认
            tcpsocket.Connect("127.0.0.1", 6000); //开启链接服务器
            tcpsocket.AsyncCheckNetThread().Forget();
        }

        public void ClientSendToServerMsg(MsgBase msg)
        {
            tcpsocket?.SendMessage(msg);
        }

       
        public void ClientStatus(NetClientStatus eventEnum)
        {
            Main.Main.EventModle.FireNow(new ClientStatusEventArgs
            {
                Sender = this,
                netClientStatus = eventEnum
            });
        }

        public void ReceiveMsgCallBack(MsgBase msgBase)
        {
            Main.Main.EventModle.FireNow(new ClientReceivesMSGFromServer
            {
                Sender = this,
                msg = msgBase
            });
        }
        public void ClientSendToServerMsg(object sender, EventArgsBase eventArgsBase)
        {
            if (eventArgsBase is ServerToClientMsgEventArgs args)
                ClientSendToServerMsg(args.msgBase);
        }
    }
}

