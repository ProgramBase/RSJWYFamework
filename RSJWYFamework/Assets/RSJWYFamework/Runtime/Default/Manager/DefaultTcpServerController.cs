using System;
using System.Net;
using RSJWYFamework.Runtime.Event;
using RSJWYFamework.Runtime.Module;
using RSJWYFamework.Runtime.Net.Public;
using RSJWYFamework.Runtime.NetWork.Base;
using RSJWYFamework.Runtime.NetWork.Event;
using RSJWYFamework.Runtime.NetWork.TCP.Server;

namespace RSJWYFamework.Runtime.Default.Manager
{
    /// <summary>
    /// 服务器模块管理器，用于和Unity之间交互
    /// </summary>
    public class DefaultTcpServerController : ISocketTCPServerController, IModule
    {
        private TcpServerService tcpsocket;


        public void Init()
        {
            Main.Main.EventModle.BindEvent<ServerToClientMsgEventArgs>(SendMsgToClient);
            Main.Main.EventModle.BindEvent<ServerToClientMsgAllEventArgs>(SendMsgToClientAll);
            
            //检查是不是监听全部IP
            tcpsocket = new();
            tcpsocket.SocketTcpClientController = this;
        }

        public void Close()
        {
            Main.Main.EventModle.UnBindEvent<ServerToClientMsgEventArgs>(SendMsgToClient);
            Main.Main.EventModle.UnBindEvent<ServerToClientMsgAllEventArgs>(SendMsgToClientAll);
            tcpsocket?.Quit();
        }

        public void Update(float time, float deltaTime)
        {
        }

        public void UpdatePerSecond(float time)
        {
        }


        public void SendMsgToClientAll(MsgBase msgBase)
        {
        }

        public void InitServer(string ip = "any", int port = 6000)
        {
            if (ip != "any")
            {
                //指定IP
                //检查IP和Port是否合法
                if (Utility.Utility.SocketTool.MatchIP(ip) && Utility.Utility.SocketTool.MatchPort(port))
                {
                    tcpsocket.Init(ip, port);
                    return;
                }
            }
            else
            {
                //监听全部IP
                //检查Port是否合法
                if (Utility.Utility.SocketTool.MatchPort(port))
                {
                    tcpsocket.Init(IPAddress.Any, port);
                    return;
                }
            }

            //全部错误则使用默认参数
            tcpsocket.Init(IPAddress.Any, 6000);
        }
        public void Update()
        {
            //TcpServerService.instance.TCPUpdate();
        }

       
        public void SendMsgToClient(object sender, EventArgsBase eventArgsBase)
        {
            if (eventArgsBase is ServerToClientMsgEventArgs args)
                SendMsgToClient(args.msgBase);
        }

        
        public void SendMsgToClientAll(object sender, EventArgsBase eventArgsBase)
        {
            if (eventArgsBase is not ServerToClientMsgAllEventArgs args) return;
            foreach (var cs in TcpServerService.ClientDic)
            {
                var msg = args.msgBase;
                msg.targetSocket = cs.Value.socket;
                SendMsgToClient(msg);
            }
        }

        public void SendMsgToClient(MsgBase msgBase)
        {
            tcpsocket?.SendMessage(msgBase);
        }


        public void ClientConnectedCallBack(ClientSocket _clientSocket)
        {
            var _event= Main.Main.ReferencePoolManager.Get<ServerClientConnectedCallBackEventArgs>();
            _event.Sender = this;
            _event.clientSocket = _clientSocket;
            Main.Main.EventModle.FireNow(_event);
        }

        public void ClientReConnectedCallBack(ClientSocket _clientSocket)
        {
            var _event= Main.Main.ReferencePoolManager.Get<ServerClientReConnectedCallBackEventArgs>();
            _event.clientSocket = _clientSocket;
            _event.Sender = this;
            Main.Main.EventModle.FireNow(_event);
        }

        public void ServerServiceStatus(NetServerStatus netServerStatus)
        {
            var _event= Main.Main.ReferencePoolManager.Get<ServerStatusEventArgs>();
            _event.Sender = this;
            _event.status = netServerStatus;
            Main.Main.EventModle.FireNow( _event);
        }

        public void FromClientReceiveMsgCallBack(ClientSocket _clientSocket, MsgBase msgBase)
        {
            var _event= Main.Main.ReferencePoolManager.Get<FromClientReceiveMsgCallBackEventArgs>();
            _event.Sender = this;
            _event.clientSocket = _clientSocket;
            _event.msgBase = msgBase;
            Main.Main.EventModle.FireNow(_event);
        }
    }
}