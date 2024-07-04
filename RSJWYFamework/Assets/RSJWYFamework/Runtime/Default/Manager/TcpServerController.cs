using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Default.EventsLibrary;
using RSJWYFamework.Runtime.Event;
using RSJWYFamework.Runtime.Module;
using RSJWYFamework.Runtime.Net.Public;
using RSJWYFamework.Runtime.NetWork.Base;
using RSJWYFamework.Runtime.NetWork.Event;
using RSJWYFamework.Runtime.NetWork.TCP.Server;
using RSJWYFamework.Runtime.NetWork.UDP;
using RSJWYFamework.Runtime.Utility;

namespace RSJWYFamework.Runtime.Default.Manager
{
    /// <summary>
    /// 服务器模块管理器，用于和Unity之间交互
    /// </summary>
    public class TcpServerController : ISocketTCPServerController, IModule
    {
        private TcpServerService tcpsocket;


        public void Init()
        {
            Main.Main.Instance.GetModule<DefaultEvenManager>().BindEvent<ServerToClientMsgEventArgs>(SendMsgToClient);
            Main.Main.Instance.GetModule<DefaultEvenManager>().BindEvent<ServerToClientMsgAllEventArgs>(SendMsgToClientAll);
            
            //检查是不是监听全部IP
            tcpsocket = new();
            tcpsocket.SocketTcpClientController = this;
        }

        public void Close()
        {
            Main.Main.Instance.GetModule<DefaultEvenManager>().UnBindEvent<ServerToClientMsgEventArgs>(SendMsgToClient);
            Main.Main.Instance.GetModule<DefaultEvenManager>().UnBindEvent<ServerToClientMsgAllEventArgs>(SendMsgToClientAll);
            tcpsocket?.Quit();
        }

       

        public void SendMsgToClientAll(MsgBase msgBase)
        {
            throw new NotImplementedException();
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
        void Update()
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
            Main.Main.Instance.GetModule<DefaultEvenManager>().SendEvent(this, new ServerClientConnectedCallBackEventArgs
            {
                clientSocket = _clientSocket
            });
        }

        public void ClientReConnectedCallBack(ClientSocket _clientSocket)
        {
            Main.Main.Instance.GetModule<DefaultEvenManager>().SendEvent(this, new ServerClientReConnectedCallBackEventArgs
            {
                clientSocket = _clientSocket
            });
        }

        public void ServerServiceStatus(NetServerStatus netServerStatus)
        {
            Main.Main.Instance.GetModule<DefaultEvenManager>().SendEvent(this, new ServerStatusEventArgs
            {
                status = netServerStatus
            });
        }

        public void FromClientReceiveMsgCallBack(ClientSocket _clientSocket, MsgBase msgBase)
        {
            Main.Main.Instance.GetModule<DefaultEvenManager>().SendEvent(this, new FromClientReceiveMsgCallBackEventArgs
            {
                clientSocket = _clientSocket,
                msgBase=msgBase
            });
        }
    }
}