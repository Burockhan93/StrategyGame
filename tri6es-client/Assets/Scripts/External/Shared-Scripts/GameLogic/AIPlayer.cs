using System;
using System.Collections.Generic;
using Shared.DataTypes;
using Shared.HexGrid;
using Shared.Structures;
using System.Linq;

namespace Shared.Game
{
    public class AIPlayer : Player
    {
        public HexCoordinates spawnCoordinates { get; private set; }
        public enum States { Idle, Move, Ressources, Build }

        private int counter;
        private List<RessourceType> _types = new List<RessourceType>() { RessourceType.WOOD, RessourceType.WHEAT, RessourceType.FOOD, RessourceType.COAL, RessourceType.COW, RessourceType.STONE };

        private States _state = States.Ressources;
        private HexCoordinates _target;
        private Action func;

        public AIPlayer(string name, HexCoordinates hexCoordinates) : base(name)
        {
            spawnCoordinates = hexCoordinates;
        }

        public AIPlayer(string name, Tribe tribe) : base(name, tribe)
        {

        }

        public AIPlayer(string name, Tribe tribe, TroopInventory TroopInventory, List<List<Quest>> quests, HexCoordinates hexCoordinates) : base(name, tribe, TroopInventory, quests)
        {
            spawnCoordinates = hexCoordinates;
        }

        public void DoTick()
        {
            counter++;
            Console.WriteLine("I am ticking, life is ticking...");


            switch (_state)
            {
                case States.Idle:
                    break;
                case States.Move:
                    Position = getNextCoordinate(_target);
                    Console.WriteLine(Position);
                    if (Position == _target)
                    {
                        func();
                        _state = States.Ressources;
                    }


                    break;
                case States.Ressources:
                    RessourceType res = ShortRessource();
                    HexCoordinates target = FindRessource(res);
                    //HexCoordinates target = FindRessource(RessourceType.FOOD);
                    Console.WriteLine(res.ToString() + " " + target.ToString());
                    _state = States.Move;
                    _target = target;
                    func = () => Console.WriteLine("lo");
                    break;
                case States.Build:
                    break;
                default: break;
            }
            //GameServer.ServerSend.BroadcastPlayer(this);

        }

        private HexCoordinates getNextCoordinate(HexCoordinates target)
        {

            int x, z;
            HexCoordinates result;

            if (target.X > Position.X) x = Position.X + 1;
            else if (target.X < Position.X) x = Position.X - 1;
            else x = Position.X;

            if (target.Z > Position.Z) z = Position.Z + 1;
            else if (target.Z < Position.Z) z = Position.Z - 1;
            else z = Position.Z;

            result = new HexCoordinates(x, z);
            return result;

        }
        private RessourceType ShortRessource()
        {
            Dictionary<RessourceType, int> shortRessources = new Dictionary<RessourceType, int>();
            // foreach( in this.Tribe.tribeInventory.storageInventories)
            foreach (KeyValuePair<RessourceType, int> res in this.Tribe.tribeInventory.HQinventory.getStorage)
            {
                if (_types.Contains(res.Key))
                    shortRessources.Add(res.Key, this.Tribe.tribeInventory.HQinventory.Limits[res.Key] - res.Value);
            }

            var ressource = shortRessources.Where(i => i.Value > 0).ToList();


            if (ressource.Count < 1) return RessourceType.WOOD;


            Console.WriteLine(ressource[0].Key + " " + ressource[0].Value);

            Random rnd = new Random();
            return ressource[rnd.Next(ressource.Count)].Key;

        }

        private HexCoordinates FindRessource(RessourceType ressource)
        {
            int c = 0;
            HexCell cell = GameLogic.grid.GetCell(Position);
            //Two ways to determine where we could find ressources. First search neighboring cells, second search the entire map choose the nearest.
            //1
            //Too slow! inDepth 5 works well but bigger numbers like 10 do seem to have a problem
            List<Ressource> ressources = cell.GetNeighborStructures<Ressource>(5);
            //ressources = ressources.FindAll(x=>x.ressourceType == ressource);
            //2 searching the entire Map is also slow. however considering that the ressources are not equally distributed 
            //its a good idea to search the entire map. 

            //3 a combination of both. Until i find out what causes the problem in GetNeighbours we are gonna go with first GetNeighbour(5)
            //if that doesnt return a value then entire MAp.
            foreach (Ressource r in ressources)
            {
                c++;
                if (r.ressourceType == ressource)
                {
                    Console.WriteLine("c: " + c);
                    return r.Cell.coordinates;

                }

            }
            Console.WriteLine("c1: " + c);

            foreach (Ressource r in GameLogic.Res)
            {
                c++;
                //if (r.ressourceType == ressource && r.ManuallyHarvestable())
                if (r.ressourceType == ressource)
                {

                    Console.WriteLine("c: " + c);
                    return r.Cell.coordinates;
                }

            }
            Console.WriteLine("c2: " + c);
            //Console.WriteLine("G: "+GameLogic.Res.Count);    

            //if this is returned then we coouldnt find the ressource on the map.
            return new HexCoordinates(0, 0);
        }
        private bool AIHarvestRessource()
        {
            return false;
        }


        private Building ConstructBuilding()
        {
            return null;
        }

        private HexCoordinates FindLocationtoBuild(Building building)
        {
            return new HexCoordinates(6, 6);
        }

    }
}
