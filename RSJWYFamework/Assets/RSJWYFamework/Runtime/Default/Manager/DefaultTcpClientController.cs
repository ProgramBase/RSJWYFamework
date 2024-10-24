using System;
using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Default.EventsLibrary;
using RSJWYFamework.Runtime.Event;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Module;
using RSJWYFamework.Runtime.Net.Public;
using RSJWYFamework.Runtime.NetWork.Event;
using RSJWYFamework.Runtime.Network.Public;
using RSJWYFamework.Runtime.NetWork.TCP.Client;
using RSJWYFamework.Runtime.Socket.Base;
using RSJWYFamework.Runtime.NetWork.TCP.Server;
using RSJWYFamework.Runtime.Utility;

namespace RSJWYFamework.Runtime.Default.Manager
{
    /// <summary>
    /// 客户端的控制器
    /// </summary>
    public class DefaultTcpClientController : ISocketTCPClientController,ILife
    {
        private TcpClientService tcpsocket;
        private bool reLock = false;
        private ISocketMsgBodyEncrypt m_SocketMsgBodyEncrypt; 
        public void Init()
        {
            reLock = false;
            Main.Main.EventModle.BindEventRecord<ClientSendToServerEventArgs>(ClientSendToServerMsg);
            Main.Main.AddLife(this);
            tcpsocket = new();
            tcpsocket.SocketTcpClientController = this;
        }

        public void Close()
        {
            reLock = true;
            Main.Main.EventModle.UnBindEventRecord<ClientSendToServerEventArgs>(ClientSendToServerMsg);
            Main.Main.RemoveLife(this);
            tcpsocket?.Quit();
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
                    return;
                }
            }

            //全部匹配失败，使用默认
            tcpsocket.Connect("127.0.0.1", 6000); //开启链接服务器
        }


        public void ClientSendToServerMsg(object sender, RecordEventArgsBase eventArgsBase)
        {
            if (eventArgsBase is ClientSendToServerEventArgs args)
                ClientSendToServerMsg(args.msgBase);
        }
        public void ClientSendToServerMsg(MsgBase msg)
        {
            tcpsocket?.SendMessage(msg);
        }

       
        public void ClientStatus(NetClientStatus eventEnum)
        {
            var _event= new ClientStatusEventArgs
            {
                Sender = this,
                netClientStatus = eventEnum
            };
            Main.Main.EventModle.Fire(_event);
            
        }

        public void ReceiveMsgCallBack(MsgBase msgBase)
        {
            var _event= new ClientReceivesMSGFromServer
            {
                Sender = null,
                msgBase = msgBase
            };
            Main.Main.EventModle.Fire(_event);
        }

        public void Update(float time, float deltaTime)
        {
            tcpsocket?.TCPUpdate();
        }

        public void UpdatePerSecond(float time)
        {
            if (tcpsocket.Status==NetClientStatus.Close||tcpsocket?.Status==NetClientStatus.Fail&&reLock==false)
            {
                RSJWYLogger.Warning($"检测到服务器链接关闭，重新连接服务器");
                tcpsocket.Connect();
            }
        }

        public void FixedUpdate()
        {
        }

        public void LateUpdate()
        {
        }
    }
}

