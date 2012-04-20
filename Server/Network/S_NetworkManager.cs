using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace Demiurge.Server
{
    static class S_NetworkManager
    {
        const float TIMEOUT_S = 2.000F;  //Timeout in seconds
        const float HEARTBEAT_S = 0.020F;   //Ping frequency in seconds

        static NetServer myLidServer;

        static S_NetworkManager()
        {
            NetPeerConfiguration lidServerConfig = new NetPeerConfiguration("chat");
            lidServerConfig.Port = 2323;
            lidServerConfig.PingInterval = HEARTBEAT_S;
            lidServerConfig.ConnectionTimeout = TIMEOUT_S;
            lidServerConfig.SetMessageTypeEnabled(NetIncomingMessageType.ConnectionApproval, true);

            myLidServer = new NetServer(lidServerConfig);
            myLidServer.Start();
        }

        public static void HandleIncomingMessages()
        {
            NetIncomingMessage im;
            while ((im = myLidServer.ReadMessage()) != null)
            {
                Console.WriteLine("\nPacket received.");
                switch (im.MessageType)
                {
                    #region Debug Messages

                    case NetIncomingMessageType.DebugMessage:

                    case NetIncomingMessageType.ErrorMessage:

                    case NetIncomingMessageType.WarningMessage:

                    case NetIncomingMessageType.VerboseDebugMessage:
                        string text = im.ReadString();
                        Console.WriteLine(text);
                        break;

                    #endregion Debug Messages

                    case NetIncomingMessageType.ConnectionApproval:
                        Console.WriteLine("Connection approved: " + im.SenderConnection);
                        im.SenderConnection.Approve();
                        break;

                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();
                        string reason = im.ReadString();
                        Console.WriteLine(NetUtility.ToHexString(im.SenderConnection.RemoteUniqueIdentifier) + " " + status + ": " + reason);
                        if (status == NetConnectionStatus.Connected)        //These can hold on connected/disconnected logic
                        {
                            //On client connected logic goes here
                        }
                        //else
                        //    myIsConnected = false;      
                        break;

                    case NetIncomingMessageType.Data:
                        HandleGameMessages(im);
                        break;

                    default:
                        Console.WriteLine("Unhandled type: " + im.MessageType + " " + im.LengthBytes + " bytes " + im.DeliveryMethod + "|" + im.SequenceChannel);
                        break;
                }
            }

        }

        static void HandleGameMessages(NetIncomingMessage im)
        {
            GameMessages incomingType = (GameMessages)im.ReadByte();
            switch (incomingType)
            {
                #region CHAT

                case GameMessages.CHAT:
                    // incoming chat message from a client
                    string chat = im.ReadString();
                    Console.WriteLine("Broadcasting '" + chat + "'");
                    //Broadcast this to all connections except for the sender
                    List<NetConnection> all = myLidServer.Connections; //Get a copy of the connection list
                    all.Remove(im.SenderConnection);                 //Remove the sender from our list 
                    if (all.Count > 0) //if there is anyone in the list
                    {
                        NetOutgoingMessage om = myLidServer.CreateMessage();
                        om.Write(NetUtility.ToHexString(im.SenderConnection.RemoteUniqueIdentifier) + " said: " + chat);  //Write their ID and message into an outgoing packet
                        myLidServer.SendMessage(om, all, NetDeliveryMethod.ReliableOrdered, 0);
                    }
                    break;

                #endregion CHAT
            }

        }

        public static NetOutgoingMessage NewOutgoing
        {
            get
            {
                NetOutgoingMessage om = myLidServer.CreateMessage();
                return om;
            }
        }

        public static void Send(NetOutgoingMessage om, NetConnection recipient, NetDeliveryMethod deliveryMethod)
        {
            myLidServer.SendMessage(om, recipient, deliveryMethod);
            myLidServer.FlushSendQueue();
        }


    }
}
