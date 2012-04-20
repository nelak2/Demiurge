using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Lidgren.Network;

namespace Demiurge.Client
{
    static class C_NetworkManager
    {
        const float TIMEOUT_S = 2.000F;  //Timeout in seconds
        const float HEARTBEAT_S = 0.020F;   //Ping frequency in seconds

        #region Fields

        public static NetClient myLidClient;

        static string myServerIP = "127.0.0.1";
        static ushort myServerPort = 2323;

        #endregion Fields

        #region Properties

        public static string ServerIP
        {
            get { return myServerIP; }
            set { myServerIP = value; }
        }

        public static ushort ServerPort
        {
            get { return myServerPort; }
            set { myServerPort = value; }
        }

        public static NetPeerStatus CurrentNetPeerStatus
        {
            get
            {
                if (myLidClient != null) return myLidClient.Status;
                else return NetPeerStatus.NotRunning;
            }
        }

        public static NetConnectionStatus CurrentConnectionStatus
        {
            get
            {
                if (myLidClient != null) return myLidClient.ConnectionStatus;
                else return NetConnectionStatus.None;
            }
        }

        #endregion Properties

        #region Methods

        #region Initialization, Connection, Disconnection

        static C_NetworkManager()     //For now, the lidClient will start with the program although it is not yet connected. PingInterval and Timeout are pre-set, but destination IP/port are handled in Connect.
        {
            NetPeerConfiguration lidClientConfig = new NetPeerConfiguration("chat");

            lidClientConfig.PingInterval = HEARTBEAT_S;
            lidClientConfig.ConnectionTimeout = TIMEOUT_S;

            myLidClient = new NetClient(lidClientConfig);

            myLidClient.Start();
        }

        public static void Connect()
        {
            if (myLidClient.ConnectionStatus == NetConnectionStatus.None || myLidClient.ConnectionStatus == NetConnectionStatus.Disconnected)
            {
                NetOutgoingMessage hail = myLidClient.CreateMessage();
                hail.Write("Yo whussup");

                myLidClient.Connect(ServerIP, ServerPort, hail);
            }
            else Console.WriteLine("Connection attempt stopped - lidClient is not totally disconnected.");
        }

        public static void Disconnect()
        {
            myLidClient.Disconnect("User decided to disconnect");
        }

        #endregion Initialization, Connection, Disconnection

        #region Handle Incoming Messages

        public static void HandleIncomingMessage()   //Handle a message from the Lidgren library. If it's a game data relevant message, execute HandleGameMessage
        {
            NetIncomingMessage im;
            while ((im = myLidClient.ReadMessage()) != null)
            {
                // handle incoming message
                switch (im.MessageType)
                {
                    case NetIncomingMessageType.DebugMessage:

                    case NetIncomingMessageType.ErrorMessage:

                    case NetIncomingMessageType.WarningMessage:

                    case NetIncomingMessageType.VerboseDebugMessage:
                        string text = im.ReadString();
                        Console.WriteLine(text);
                        break;

                    case NetIncomingMessageType.ConnectionApproval:
                        Console.WriteLine("Connection approval");
                        break;

                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();
                        //if (status == NetConnectionStatus.Connected)        //These can hold on connected/disconnected logic
                        //    myIsConnected = true;
                        //else
                        //    myIsConnected = false;
                        string reason = im.ReadString();
                        Console.WriteLine(status.ToString() + ": " + reason);
                        break;

                    case NetIncomingMessageType.Data:
                        HandleGameMessage(im);            //Branch off into function HandleGameMessages for all the game-related messages.
                        break;

                    default:
                        Console.WriteLine("Unhandled type: " + im.MessageType + " " + im.LengthBytes + " bytes");
                        break;
                }
            }
        }

        public static void HandleGameMessage(NetIncomingMessage im)
        {
            GameMessages incomingType = (GameMessages)im.ReadByte();
            switch (incomingType)
            {
                #region CHAT

                case GameMessages.CHAT:
                    string chat = im.ReadString();
                    Console.WriteLine(chat);
                    break;

                #endregion CHAT

            }
        }

        #endregion Handle Incoming

        #region Handle Outgoing Messages

        public static NetOutgoingMessage NewOutgoing
        {
            get
            {
                NetOutgoingMessage om = myLidClient.CreateMessage();
                return om;
            }
        }

        public static void SendString(string text)
        {
            NetOutgoingMessage om = myLidClient.CreateMessage();
            om.Write((byte)GameMessages.CHAT);
            om.Write(text);
            myLidClient.SendMessage(om, NetDeliveryMethod.ReliableOrdered);
            Console.WriteLine("Sending '" + text + "'");
            myLidClient.FlushSendQueue();
        }

        public static void Send(NetOutgoingMessage om, NetDeliveryMethod deliveryMethod)
        {
            myLidClient.SendMessage(om, deliveryMethod);
            myLidClient.FlushSendQueue();
        }


        #endregion Handle Outgoing

        #endregion Methods


    }
}
