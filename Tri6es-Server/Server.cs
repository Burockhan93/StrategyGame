using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
using Shared.MapGeneration;
using Shared.Game;
using Shared.Communication;



namespace GameServer
{
    class Server
    {
        public static int MaxPlayers { get;private set; }
        public static int Port { get; private set; }

        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

        public delegate void PacketHandler(int fromClient, Packet packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        private static TcpListener tcpListener;

        private static Stopwatch sw;
        private static long ping;

        private static bool askForCoordinates = false;

        public static void Start(int maxPlayers, int port)
        {
            MaxPlayers = maxPlayers;
            Port = port;

            Console.WriteLine("Starting Server...");
            InitServerData();




            if (File.Exists("savegame.hex"))
            {
                LoadGame();
            }
            else
            {
                NewGame();
            }


            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"Server Started on {Port}.");
        }

        private static void NewGame()
        {
            //Get User Input for Coordinates
           
           float lat = 49.90150875572815f;
            //float lat = 47.489221f;
           float lon = 8.68283879515591f;
           // float lon =  9.604513f;

            if (askForCoordinates)
            {
                Console.WriteLine("Please enter the latitude of your desired position or leave empty for default position...");
                string latS = Console.ReadLine();
                if (latS != "")
                {
                    lat = float.Parse(latS);
                }

                Console.WriteLine("Please enter the longtitude of your desired position or leave empty for default position...");
                string lonS = Console.ReadLine();
                if (lonS != "")
                {
                    lon = float.Parse(lonS);
                }
            }


            Console.WriteLine("Generating Map...");
            Console.WriteLine(lat + " , " + lon);
            MapGenerator mapGenerator = new MapGenerator(lat, lon, 7);

            Shared.HexGrid.HexGrid grid = mapGenerator.CreateMap();
            GameLogic.newGameInit(grid);

        }

        public static void SaveGame()
        {
            File.WriteAllBytes("savegame.hex", ServerSend.SaveGameState().ToArray());
        }

        private static void LoadGame()
        {
            Console.WriteLine("Loading Game...");
            byte[] gamestate = File.ReadAllBytes("savegame.hex");
            ServerSend.LoadGameState(new Packet(gamestate));
            Console.WriteLine("Game Loaded!");
        }

        private static void TCPConnectCallback(IAsyncResult result)
        {
            TcpClient client = tcpListener.EndAcceptTcpClient(result);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
            Console.WriteLine($"Incoming Connection from {client.Client.RemoteEndPoint}...");

            for (int i = 1; i <= MaxPlayers; i++)
            {
                if(clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(client);
                    return;
                }
            }

            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to Connect: Server full!");
        }

        private static void InitServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int) ClientPackets.welcomeReceived, ServerHandle.WelcomeRecieved },
                { (int) ClientPackets.requestHexGrid, ServerHandle.HandleMapRequest },
                { (int) ClientPackets.requestPlaceBuilding, ServerHandle.HandlePlaceBuilding },
                { (int) ClientPackets.requestUpgradeBuilding, ServerHandle.HandleUpgradeBuilding },
                { (int) ClientPackets.requestRepairBuilding, ServerHandle.HandleRepairBuilding },
                { (int) ClientPackets.positionUpdate, ServerHandle.HandlePositionUpdate  },
                { (int) ClientPackets.requestBuildHQ, ServerHandle.HandleBuildHQ  },
                { (int) ClientPackets.requestJoinTribe, ServerHandle.HandleJoinTribe },
                { (int) ClientPackets.requestMoveTroops, ServerHandle.HandleMoveTroops },
                { (int) ClientPackets.requestFight, ServerHandle.HandleFight },
                { (int) ClientPackets.requestHarvest, ServerHandle.HandleHarvest },
                { (int) ClientPackets.requestChangeAllowedRessource, ServerHandle.HandleChangeAllowedRessource },
                { (int) ClientPackets.requestChangeTroopRecipeOfBarracks, ServerHandle.HandleChangeTroopRecipeOfBarracks },
                { (int) ClientPackets.requestChangeOrderOfStrategy, ServerHandle.HandleChangeStrategy },
                { (int) ClientPackets.requestChangeActiveOfStrategyPlayer, ServerHandle.HandleChangeActiveOfStrategyPlayer },
                { (int) ClientPackets.requestChangeActiveOfStrategyBuilding, ServerHandle.HandleChangeActiveOfStrategyBuilding },
                { (int) ClientPackets.requestMoveRessource, ServerHandle.HandleMoveRessource },
                { (int) ClientPackets.requestChangeRessourceLimit, ServerHandle.HandleChangeRessourceLimit },
                { (int) ClientPackets.requestRecipeChange, ServerHandle.HandleRecipeChange },
                { (int) ClientPackets.requestUpdateMarketRessource, ServerHandle.HandleUpdateMarketRessource },
                { (int) ClientPackets.requestDestroyBuilding, ServerHandle.HandleDestroyBuilding },
                { (int) ClientPackets.sendChatMessage, ServerHandle.HandleSendChatMessage },
                { (int) ClientPackets.requestChatHistory, ServerHandle.HandleRequestChatHistory },
                { (int) ClientPackets.sendMovementQuestProgress, ServerHandle.HandleMovementQuestProgress },
                { (int) ClientPackets.sendBuildingQuestProgress, ServerHandle.HandleBuildingQuestProgress },
                { (int) ClientPackets.sendRessourceQuestProgress, ServerHandle.HandleRessourceQuestProgress },
                { (int) ClientPackets.requestLeaveTribe, ServerHandle.HandleLeaveTribe },
                { (int) ClientPackets.requestSalvageBuilding, ServerHandle.HandleSalvageBuilding },
                { (int) ClientPackets.requestPrioChange, ServerHandle.HandlePrioChange },
                { (int) ClientPackets.requestCreateGuild, ServerHandle.HandleCreateGuild },
                { (int) ClientPackets.requestLeaveGuild, ServerHandle.HandleLeaveGuild },
                { (int) ClientPackets.requestJoinGuild, ServerHandle.HandleJoinGuild },
                { (int) ClientPackets.requestGuildDonation, ServerHandle.HandleGuildDonation },
                { (int) ClientPackets.requestDepositToGuild, ServerHandle.HandleDepositToGuild },
                { (int) ClientPackets.requestWithdrawalFromGuild, ServerHandle.HandleWithdrawalFromGuild },
                { (int) ClientPackets.requestBoostBaseResourceSelectionGuild, ServerHandle.HandleBoostBaseResourceSelectionGuild },
                { (int) ClientPackets.requestBoostAdvancedResourceSelectionGuild, ServerHandle.HandleBoostAdvancedResourceSelectionGuild },
                { (int) ClientPackets.requestBoostWeaponSelectionGuild, ServerHandle.HandleBoostWeaponSelectionGuild },
                { (int) ClientPackets.requestCollect, ServerHandle.HandleCollect },
                { (int) ClientPackets.requestResearch, ServerHandle.HandleResearch },
            };
            Console.WriteLine($"Initialized packets");
        }

        public static void StartPingTest()
        {
            sw = new Stopwatch();
            sw.Start();
        }

        public static void StopPingTest(int ClientID)
        {
            sw.Stop();
            ping = sw.ElapsedMilliseconds / 2;

            Console.WriteLine($"Ping to Client {ClientID}: {ping}ms");

            ServerSend.Ping(ClientID, ping);
        }
    }
}
