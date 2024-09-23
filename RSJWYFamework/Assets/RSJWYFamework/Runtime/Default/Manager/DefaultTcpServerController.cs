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
        private TCPServer tcpsocket;


        public void Init()
        {
            Main.Main.EventModle.BindEventRecord<ServerToClientMsgEventArgs>(SendMsgToClientEvent);
            Main.Main.EventModle.BindEventRecord<ServerToClientMsgAllEventArgs>(SendMsgToClientAllEvent);
            
            //检查是不是监听全部IP
            tcpsocket = new();
            tcpsocket.SocketTcpServerController = this;
        }

        public void Close()
        {
            Main.Main.EventModle.UnBindEventRecord<ServerToClientMsgEventArgs>(SendMsgToClientEvent);
            Main.Main.EventModle.UnBindEventRecord<ServerToClientMsgAllEventArgs>(SendMsgToClientAllEvent);
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

       
        public void SendMsgToClientEvent(object sender, RecordEventArgsBase eventArgsBase)
        {
            if (eventArgsBase is ServerToClientMsgEventArgs args)
                SendMsgToClient(args.msgBase,args.ClientSocketToken);
        }

        
        public void SendMsgToClientAllEvent(object sender, RecordEventArgsBase eventArgsBase)
        {
            if (eventArgsBase is not ServerToClientMsgAllEventArgs args) return;
            foreach (var token in TcpServerService.ClientDic)
            {
                var msg = args.msgBase;
                SendMsgToClient(msg,token.Value);
            }
        }

        public void SendMsgToClient(MsgBase msgBase,ClientSocketToken clientSocketToken)
        {
            tcpsocket?.SendMessage(msgBase,clientSocketToken);
        }


        public void ClientConnectedCallBack(ClientSocketToken clientSocketToken)
        {
            var _event = new ServerClientConnectedCallBackEventArgs
            {
                Sender = this,
                ClientSocketToken = clientSocketToken,
                msgBase = null
            };
            Main.Main.EventModle.Fire(_event);
        }

        public void ClientReConnectedCallBack(ClientSocketToken clientSocketToken)
        {
            var _event = new ServerClientReConnectedCallBackEventArgs
            {
                Sender = this,
                ClientSocketToken = clientSocketToken,
                msgBase = null
            };
            Main.Main.EventModle.Fire(_event);
        }

        public void ServerServiceStatus(NetServerStatus netServerStatus)
        {
            var _event = new ServerStatusEventArgs
            {
                Sender = this,
                status = netServerStatus
            };
            Main.Main.EventModle.Fire( _event);
        }

        public void FromClientReceiveMsgCallBack(ClientSocketToken clientSocketToken, MsgBase msgBase)
        {
            var _event= new FromClientReceiveMsgCallBackEventArgs
            {
                Sender = this,
                ClientSocketToken = clientSocketToken,
                msgBase = msgBase
            };
            Main.Main.EventModle.Fire(_event);
        }
    }
}