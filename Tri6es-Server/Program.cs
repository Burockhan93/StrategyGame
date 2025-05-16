using System;
using System.Threading;
using Shared.Communication;
using Shared.Game;

namespace GameServer
{
    class Program
    {
        private static bool isRunning = false;

        static void Main(string[] args)
        {
            Console.Title = "Game Server";
            isRunning = true;

            #if DEBUG
                System.Console.WriteLine("Running in DEBUG mode");
            #else
                System.Console.WriteLine("Running in RELEASE mode");
            #endif

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();
            //42069
            Server.Start(50, 42069);
        }

        private static void MainThread()
        {
            Console.WriteLine($"Main Thread started. Running at {Constants.TICKS_PER_SEC} ticks per second.");
            DateTime nextLoop = DateTime.Now;
            DateTime saveGame = DateTime.Now;
            int counter = 0;

            while (isRunning)
            {
                while(nextLoop < DateTime.Now)
                {
                    GameServer.MainThread.Update();

                    counter++;
                    if (counter == Constants.COUNTER_MAX)
                    {
                        
                        GameLogic.DoTick();
                        ServerSend.SendGameTick();
                        counter = 0;
                    }

                  
                    if((DateTime.Now.Subtract(saveGame).TotalSeconds) > Constants.SAVE_RATE) 
                    {
                        Console.WriteLine("Save");
                        Server.SaveGame();
                        saveGame = DateTime.Now;
                    }
                    nextLoop = nextLoop.AddMilliseconds(Constants.MS_PER_TICK);
                    DateTime now = DateTime.Now;
                    if (nextLoop > now)
                    {
                        Thread.Sleep(nextLoop - now);
                    }
                }
            }
        }
    }
}
