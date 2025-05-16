using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using Shared.Communication;
using Shared.Game;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBufferSize = 4096;

    public string ip = "127.0.0.1";
    public int port = 26950;
    public int myid = 0;
    public TCP tcp;

    private bool isConnected = false;
    private delegate void PacketHandler(Packet packet);
    private static Dictionary<int, PacketHandler> packetHandlers;

    public Player Player;

    public int receivedPackages = 0;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Debug.Log("Instance already exists ,destroying obj!");
            Destroy(this);
        }
    }

    private void Start()
    {
        tcp = new TCP();

        ConnectToServer();
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    public void ConnectToServer()
    {
        if (isConnected)
        {
            Disconnect();
        }
        ip = PlayerPrefs.GetString("IP");
        port = int.Parse(PlayerPrefs.GetString("Port"));

        InitClientData();
        isConnected = true;
        tcp.Connect();
    }

    public class TCP
    {
        public TcpClient socket;

        private NetworkStream stream;
        private Packet recievedData;
        private byte[] recieveBuffer;

        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            recieveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult result)
        {
            try
            {
                socket.EndConnect(result);
            }
            catch
            {
                return;
            }

            if (!socket.Connected)
            {
                return;
            }

            stream = socket.GetStream();
            stream.ReadTimeout = 10000;
            stream.WriteTimeout = 10000;

            recievedData = new Packet();

            stream.BeginRead(recieveBuffer, 0, dataBufferSize, RecieveCallback, null);
        }

        public void SendData(Packet packet)
        {
            try
            {
                if(socket != null)
                {
                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            }
            catch (Exception e)
            {
                if (e is System.IO.IOException)
                {
                    FindObjectOfType<LoadManager>().LoadTitleScreen();
                }
                Debug.Log($"Error sending Data to the server via TCP: {e}.");
            }
        }

        private void RecieveCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = stream.EndRead(result);
                if (byteLength <= 0)
                {
                    instance.Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(recieveBuffer, data, byteLength);

                recievedData.Reset(HandleData(data));
                stream.BeginRead(recieveBuffer, 0, dataBufferSize, RecieveCallback, null);
            }
            catch(Exception e)
            {
                if (e is System.IO.IOException)
                {
                    FindObjectOfType<LoadManager>().LoadTitleScreen();
                }
                Disconnect();
            }
        }

        private bool HandleData(byte[] data)
        {
            int packetLength = 0;

            recievedData.SetBytes(data);

            if(recievedData.UnreadLength() >= 4)
            {
                packetLength = recievedData.ReadInt();
                if(packetLength <= 0)
                {
                    return true;
                }
            }

            while(packetLength > 0 && packetLength <= recievedData.UnreadLength())
            {
                byte[] packetBytes = recievedData.ReadBytes(packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(packetBytes))
                    {
                        Client.instance.receivedPackages++;

                        int packetID = packet.ReadInt();
                        packetHandlers[packetID](packet);
                    }
                });

                packetLength = 0;

                if (recievedData.UnreadLength() >= 4)
                {
                    packetLength = recievedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if(packetLength <= 1)
            {
                return true;
            }

            return false;
        }

        private void Disconnect()
        {
            instance.Disconnect();

            stream = null;
            recievedData = null;
            recieveBuffer = null;
            socket = null;
        }
    }

    private void InitClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            {(int)ServerPackets.welcome, ClientHandle.Welcome },
            {(int)ServerPackets.ping, ClientHandle.Ping },
            {(int)ServerPackets.initGameLogic, ClientHandle.InitGameLogic },
            {(int)ServerPackets.upgradeBuilding, ClientHandle.UpgradeBuilding },
            {(int)ServerPackets.repairBuilding, ClientHandle.RepairBuilding },
            {(int)ServerPackets.gameTick, ClientHandle.ReceiveGameTick },
            {(int)ServerPackets.broadcastPlayer, ClientHandle.ReceivePlayer },
            {(int)ServerPackets.broadcastTribe, ClientHandle.ReceiveTribe },
            {(int)ServerPackets.applyBuild, ClientHandle.ApplyBuild },
            {(int)ServerPackets.applyBuildHQ, ClientHandle.ApplyBuildHQ },
            {(int)ServerPackets.broadcastMoveTroops, ClientHandle.HandleMoveTroops },
            {(int)ServerPackets.broadcastFight, ClientHandle.HandleFight },
            {(int)ServerPackets.broadcastHarvest, ClientHandle.HandleHarvest },
            {(int)ServerPackets.broadcastChangeAllowedRessource, ClientHandle.HandleChangeAllowedRessource },
            {(int)ServerPackets.broadcastChangeTroopRecipeOfBarracks, ClientHandle.HandleChangeTroopRecipeOfBarracks },
            {(int)ServerPackets.broadcastChangeStrategyOfProtectedBuilding, ClientHandle.HandleChangeStrategyOfProtectedBuilding },
            {(int)ServerPackets.broadcastChangeStrategyOfName, ClientHandle.HandleChangeStrategyOfPlayer },
            {(int)ServerPackets.broadcastChangeStrategyActivePlayer, ClientHandle.HandleChangeStrategyActivePlayer },
            {(int)ServerPackets.broadcastChangeStrategyActiveBuilding, ClientHandle.HandleChangeStrategyActiveBuilding },
            {(int)ServerPackets.broadcastMoveRessources, ClientHandle.HandleMoveRessources },
            {(int)ServerPackets.broadcastChangeRessourceLimit, ClientHandle.HandleChangeRessourceLimit },
            {(int)ServerPackets.broadcastUpdateMarketRessource, ClientHandle.HandleUpdateMarketRessource },
            {(int)ServerPackets.broadcastDestroyBuilding, ClientHandle.HandleDestroyBuilding },
            {(int)ServerPackets.sendChatHistory, ClientHandle.HandleChatHistory },
            {(int)ServerPackets.broadcastChatMessage, ClientHandle.HandleChatMessage},
            {(int)ServerPackets.broadcastRecipeChange, ClientHandle.HandleRecipeChange },
            {(int)ServerPackets.sendQuests, ClientHandle.HandleQuests },
            {(int)ServerPackets.salvageBuilding, ClientHandle.HandleSalvage},
            {(int)ServerPackets.prioChange, ClientHandle.HandlePrioChange},
            {(int)ServerPackets.broadcastWeather, ClientHandle.HandleWeather},
            {(int)ServerPackets.createGuild, ClientHandle.CreateGuild},
            {(int)ServerPackets.leaveGuild, ClientHandle.LeaveGuild},
            {(int)ServerPackets.joinGuild, ClientHandle.JoinGuild},
            {(int)ServerPackets.guildDonation, ClientHandle.GuildDonation},
            {(int)ServerPackets.depositToGuild, ClientHandle.DepositToGuild},
            {(int)ServerPackets.withdrawalFromGuild, ClientHandle.WithdrawalFromGuild},
            {(int)ServerPackets.boostBaseResourceSelectionGuild, ClientHandle.BoostBaseResourceSelectionGuild},
            {(int)ServerPackets.boostAdvancedResourceSelectionGuild, ClientHandle.BoostAdvancedResourceSelectionGuild},
            {(int)ServerPackets.boostWeaponsSelectionGuild, ClientHandle.BoostWeaponSelectionGuild},
            {(int)ServerPackets.broadcastCollect, ClientHandle.HandleCollect},
            {(int)ServerPackets.broadcastResearch, ClientHandle.HandleResearch},
        };
        Debug.Log("Initialized Client Data!");
    }

    public void Disconnect()
    {
        if (isConnected)
        {
            isConnected = false;
            tcp.socket.Close();

            Debug.Log("Disconnected from server.");
        }
    }
}
