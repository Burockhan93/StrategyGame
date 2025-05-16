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
        public enum States { Idle,Move,Ressources,Build,Collectable}
        
        private int counter;
        private List<RessourceType> _searchableRessources = new List<RessourceType>() { RessourceType.WOOD, RessourceType.WHEAT, RessourceType.FOOD, RessourceType.COAL, RessourceType.COW, RessourceType.STONE };
        private List<Type> _buildableStructures = new List<Type>() {    typeof(Bridge),
                                                                        typeof(CowFarm),
                                                                        typeof(Fisher),
                                                                        typeof(Quarry),
                                                                        typeof(LandRoad),
                                                                        typeof(Woodcutter),
                                                                        typeof(WheatFarm)
        };

        //Main Logic Parameters
        private Queue<States> stateLogics = new Queue<States>();
        private States _state = States.Build;
        private HexCoordinates _target;
        private List<HexCell> _currentPath;
        IEnumerator<HexCell> _currentPathEnumerator;
        private Action func;
        private Random random = new Random();
        //State Avatars. These are hardcoded under the UI Manager in Client Side 
        byte _ressource = 13; byte _move = 12; byte _coll = 14; byte _build = 15; byte _idle = 16;

        #region Constructors
        public AIPlayer(string name,HexCoordinates hexCoordinates) :base (name)
        {
            spawnCoordinates = hexCoordinates;
        }

        public AIPlayer(string name, Tribe tribe) : base(name,tribe)
        {
            
        }

        public AIPlayer(string name, Tribe tribe, TroopInventory TroopInventory, List<List<Quest>> quests, HexCoordinates hexCoordinates) : base(name,tribe,TroopInventory,quests)
        {
            spawnCoordinates = hexCoordinates;
        }
        #endregion

        public override void  DoTick()
        {
            if (stateLogics.Count < 1) CreateLogics();
          
            //Console.WriteLine("state "+_state);       
            

            switch (_state)
            {
                case States.Idle:
                    //If Idle wait 10 ticks
                    this.Avatar = _idle;
                    counter++;
                    if (counter >= 10) { counter = 0; _state = stateLogics.Dequeue(); }
                    break;
                case States.Move:
                    StateMove();
                   
                    break;
                case States.Ressources:
                    StateRessource();
                    
                    break;
                case States.Build:
                    StateBuild();

                    break;
                case States.Collectable:
                    StateCollectable();
          
                    break;
                default: break;
            }
            GameServer.ServerSend.BroadcastPlayer(this);
            
        }
        void StateRessource(RessourceType? res =null)
        {
            this.Avatar = _ressource;
            RessourceType ressource;
            HexCoordinates? hexCoordinates;
            if(!res.HasValue) ressource = ShortRessource(); else ressource = res.Value;
            hexCoordinates = FindRessource(ressource);
            if (!hexCoordinates.HasValue) return;

            ////AStarPathFinding 
            //HexCell end = GameLogic.grid.GetCell(target);
            //HexCell start = GameLogic.grid.GetCell(this.Position);
            //_currentPath = Pathfinding.FindPath(start, end);
            //foreach(HexCell coordinates in _currentPath) Console.WriteLine(coordinates.coordinates);
            //_currentPathEnumerator = _currentPath.GetEnumerator();

            Console.WriteLine(res.ToString() + " " + hexCoordinates.ToString());
            //If we called Stateressource from StateBuild, then upon harvestin ressource call build again.
            if (res.HasValue)
            {
                func = () =>
                {
                    Feedback f = new Feedback();
                    if (GameLogic.Harvest(this.Name, hexCoordinates.Value, ref f) != 0)
                        GameServer.ServerSend.BroadcastHarvest(this, hexCoordinates.Value, f);

                };
                StateMove(States.Build);
                return;
            } 
            _state = States.Move;
            _target = hexCoordinates.Value;

            func = () =>
            {
                Feedback f = new Feedback(Feedback.FeedbackStyle.harvest);
                if (GameLogic.Harvest(this.Name, hexCoordinates.Value, ref f) != 0)
                    GameServer.ServerSend.BroadcastHarvest(this, hexCoordinates.Value, f);
                Console.WriteLine("** " + f.resource);
               
            };
        }
        void StateBuild()
        {
            this.Avatar = _build;
            HexCoordinates? hexCoordinates;
            Type t = DecideBuilding();
            if (t == null) { Console.WriteLine("null building returning..."); return; }

            Building building = (Building)Activator.CreateInstance(t);
            Console.WriteLine(building.GetFriendlyName());

            //Some ressources to build are not located in the map. They need to be manufactured. This will only be possible when we have Level 2 HQ. So we are jumping over it.
            #region BUILDING_NEED_RESSOURCE
            //RessourceType? res = Tribe.tribeInventory.NeededRessources(building.Recipes[building.Level]);
            //Console.WriteLine("res "+ res.ToString());
            //if (res!=null)
            //{
            //    //We have a lacking ressource first find it.
            //    StateRessource(res.Value);
            //    return;
            //}
            #endregion


            if ((building is Bridge) || (building is LandRoad))
            {
                Buildpath();
                _state = stateLogics.Dequeue();
                return ;

            }
            hexCoordinates = FindLocationtoBuild(building); 
            if (hexCoordinates == null) { Console.WriteLine("null coor returning..."); return; }

            _target = hexCoordinates.Value;
            _state = States.Move;

            func = () =>
            {
                Feedback f = new Feedback(Feedback.FeedbackStyle.build);
                if (GameLogic.ApplyBuild(hexCoordinates.Value, t, this.Tribe) != null)
                    GameServer.ServerSend.BroadcastApplyBuild(hexCoordinates.Value, t, this, f);
            };
        }
        void StateCollectable()
        {
            this.Avatar = _coll;
            HexCoordinates? hexCoordinates;
            hexCoordinates = FindCollectable();
            if (!hexCoordinates.HasValue) return;
            _target = hexCoordinates.Value;
            _state = States.Move;
            func = () =>
            {
                Feedback f = new Feedback(Feedback.FeedbackStyle.collect);
                if (GameLogic.Collect(this.Name, hexCoordinates.Value, ref f))
                    GameServer.ServerSend.broadcastCollect(this, hexCoordinates.Value, f);
            };
        }
        void StateMove(States? state =null)
        {
            _state= States.Move;
            //this.Avatar = _move;
            Position = getNextCoordinate(_target);
            //A* algorithm works but simple solution yields a faster route;
            //HexCell hexCell = getNextCell();
            //if(hexCell!=null) Position=hexCell.coordinates;
            Console.WriteLine(Position);
            if (Position == _target)
            {
                if (func!=null) func();
                func = null;
                if(state.HasValue) _state = state.Value;
                else               _state =  stateLogics.Dequeue();
            }
        }

       
        private HexCoordinates? FindCollectable()
        {
            HexCell cell;
            try
            {
                cell = GameLogic.grid.cells.Where(x => x.Structure is Collectable).OrderBy(x => HexCoordinates.calcDistance(x.coordinates, this.Position)).First();
            }catch(Exception ex) { cell = null; }

            if (cell != null)
            {
                Console.WriteLine(cell.coordinates);
                return cell.coordinates;
            }
            return null;
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
        private HexCell getNextCell()
        {
          if(_currentPathEnumerator.MoveNext()) return _currentPathEnumerator.Current;
          else return null;
        }
        private RessourceType ShortRessource()
        {
            Dictionary<RessourceType, int> shortRessources = new Dictionary<RessourceType, int>();

            foreach(KeyValuePair<RessourceType, int> res in this.Tribe.tribeInventory.HQinventory.getStorage)
            {
                if(_searchableRessources.Contains(res.Key))
                shortRessources.Add(res.Key, this.Tribe.tribeInventory.HQinventory.Limits[res.Key] - res.Value);
            }

           var ressource = shortRessources.Where(i=>i.Value>0).ToList();
            //if ressources are full, search for wood anyways.
            if(ressource.Count<1) return RessourceType.WOOD;
            return ressource[random.Next(ressource.Count)].Key;
           
        }

        private HexCoordinates? FindRessource(RessourceType ressource)
        {
            HexCell cell;
            try
            {
                cell = GameLogic.grid.cells.Where(x => (x.Structure is Ressource)
                && ((x.Structure as Ressource).ressourceType == ressource)
                && ((x.Structure as Ressource).Harvestable())).OrderBy(x => HexCoordinates.calcDistance(x.coordinates, this.Position))?.First();
            }catch(Exception e) { cell = null; }
            if (cell != null)
            {
                Console.WriteLine(ressource.ToString() +" "+ cell.coordinates);
                return cell.coordinates;
            }
            else
            {
                return null;
            }
        }
       
        private Type DecideBuilding()
        {
            //1-building for AI doesnt need ressources or alternatively need Ressources
            //2-Baiscally if there are no buildings yet dont built a road or a bridge.count<1 refers to that.
            List<Type> types = new List<Type>();
            types = Tribe.BuildingLimits[Tribe.HQ.Level-1].Keys.Where(x =>Tribe.CurrentBuildings.ContainsKey(x)
            && Tribe.CurrentBuildings[x] != null
            && Tribe.CurrentBuildings[x].Count< Tribe.BuildingLimits[Tribe.HQ.Level-1][x]).ToList();

            Console.WriteLine("numbers: "+types.Count);
            int count = 0; bool full = true;
            for (int i = 0; i < types.Count; i++)
            {
                //Check how many building more can be built apart from road or bridge
                if ((!types[i].IsAssignableFrom(typeof(LandRoad)) || !types[i].IsAssignableFrom(typeof(Bridge))))  count += Tribe.CurrentBuildings[types[i]].Count;
                //if there are still slots to build, we could still build more building
                //if (Tribe.CurrentBuildings[_buildableStructures[i]].Count < Tribe.BuildingLimits[Tribe.HQ.Level-1][_buildableStructures[i]]) full = false;
                if (Tribe.CurrentBuildings[types[i]].Count < Tribe.BuildingLimits[Tribe.HQ.Level-1][types[i]]) full = false;

            }
            Console.WriteLine("count :"+count);
            if (full) return null;
            //We have buildings so we can build a road/bridge as well
            if (count > 0) return types[random.Next(0, types.Count)];
            //If we dont then buildings except Road/Bridge can be built
            else { return types.Where(x => !x.IsAssignableFrom(typeof(LandRoad)) && !x.IsAssignableFrom(typeof(Bridge))).ToList()[random.Next(0, types.Count - 2)]; } 


        }

        private HexCoordinates? FindLocationtoBuild(Building building)
        {
            HexCell[] cells = GameLogic.grid.cells;
            List<HexCell> ourCells = new List<HexCell>();
            foreach (HexCell cell in cells)
            {
                //buildings cann only be built on our territory
                if (cell.GetCurrentTribe()== this.Tribe.Id) ourCells.Add(cell);
            }

            if (building is CowFarm)
            {
                Console.WriteLine("CowFarm...");
                foreach (HexCell cell in ourCells)
                {
                    if (cell.Data.Biome == HexCellBiome.GRASS && !(cell.Structure is Building) && (cell.Structure is Cow))
                    {
                        return cell.coordinates;
                    }
                    else if ((cell.Data.Biome == HexCellBiome.GRASS && !(cell.Structure is Building)))
                    {
                        return cell.coordinates;   
                    }
                }
                return null;
            }
            else if (building is Fisher)
            {
                Console.WriteLine("Fisher...");
                foreach (HexCell cell in ourCells)
                {
                    if(cell.Data.Biome == HexCellBiome.WATER )
                    {
                        //List<HexCell> buildLocs = cell.GetNeighbors(1);
                        List<HexCell> buildLocs = cell.neighbors.ToList();
                        foreach(HexCell cell2 in buildLocs)
                        {
                            if (cell2.GetCurrentTribe()==this.Tribe.Id && !(cell2.Structure is Building) && cell2.Data.Biome != HexCellBiome.WATER)
                            {
                                return cell2.coordinates;
                            }
                        }
                    }
                }
            }
            else if (building is Quarry)
            {
                Console.WriteLine("Quarry...");
                foreach (HexCell cell in ourCells)
                {
                    if (cell.Data.Biome == HexCellBiome.ROCK && !(cell.Structure is Building) && (cell.Structure is Rock))
                    {
                        return cell.coordinates;
                    }
                    else if ((cell.Data.Biome == HexCellBiome.ROCK && !(cell.Structure is Building)))
                    {
                        return cell.coordinates;
                    }
                }
               
                return null;
            }
            else if (building is WheatFarm)
            {
                Console.WriteLine("WheatFarm...");
                foreach (HexCell cell in ourCells)
                {
                    if (cell.Data.Biome == HexCellBiome.CROP && !(cell.Structure is Building) && (cell.Structure is Wheat))
                    {
                        return cell.coordinates;
                    }
                    else if ((cell.Data.Biome == HexCellBiome.CROP && !(cell.Structure is Building)))
                    {
                        return cell.coordinates;
                    }
                }
                return null;
            }
            else if (building is Woodcutter)
            {
                Console.WriteLine("WoodCutter...");
                HexCoordinates? coor = null;
                foreach (HexCell cell in ourCells)
                {
                    if ((cell.Data.Biome == HexCellBiome.FOREST || cell.Data.Biome == HexCellBiome.SCRUB) && !(cell.Structure is Building) && (cell.Structure is Tree))
                    {
                        return cell.coordinates;
                    }
                    else if ((cell.Data.Biome == HexCellBiome.FOREST || cell.Data.Biome == HexCellBiome.SCRUB) && !(cell.Structure is Building))
                    {
                        coor = cell.coordinates;
                    }
                }
                return coor;
            }
            return null;
        }
        void Buildpath()
        {
            //To build a path we first need a building as a startpoint 
            // from the cureentbuilding we choose - non road-non bridge type then we make sure we had this type of building in our currentbuildings
            List<Building> availableBuildingsWithNoRoad = new List<Building>();
            foreach (Type t in Tribe.CurrentBuildings.Keys)
            {
                if ((t.IsAssignableFrom(typeof(LandRoad)) || t.IsAssignableFrom(typeof(Bridge))) && Tribe.CurrentBuildings[t] == null && Tribe.CurrentBuildings[t].Count < 1) continue;
                foreach (Building buil in Tribe.CurrentBuildings[t])
                {
                    foreach(HexCell cell in buil.Cell.GetNeighbors(1))
                    {
                        // If a building has a road or a bridge to its adjacent cells dont consider it, because it probably already has a path.
                        if (!(cell.Structure is LandRoad || cell.Structure is Bridge) && !availableBuildingsWithNoRoad.Contains(buil)  ) availableBuildingsWithNoRoad.Add(buil);
                    }   
                    
                }
            }
            
            Building building = availableBuildingsWithNoRoad[random.Next(availableBuildingsWithNoRoad.Count)];
            Console.WriteLine("Building: " + building.ToString());
            Console.WriteLine("celss: " + building.Cell.coordinates + " " + "celss2: " + Tribe.HQ.Cell.coordinates);
            //create a path, check if its valid, f is the degree of effort for every cell
            List<HexCell> cellsToPath = Pathfinding.FindPath(building.Cell, Tribe.HQ.Cell, false);
            foreach (HexCell cell in cellsToPath) { Console.WriteLine(cell.coordinates + " " + cell.f); }

            //built everything at once, if biome is water build bridge, dont built the first and last location of path because they are buildings
            for (int i =1; i < cellsToPath.Count - 1; i++)
            {
                Feedback f = new Feedback();
                if (cellsToPath[i].Data.Biome == HexCellBiome.WATER)
                {
                    if (GameLogic.ApplyBuild(cellsToPath[i].coordinates, typeof(Bridge), this.Tribe) != null)
                        GameServer.ServerSend.BroadcastApplyBuild(cellsToPath[i].coordinates, typeof(Bridge), this, f);
                }
                else
                {
                    if (GameLogic.ApplyBuild(cellsToPath[i].coordinates, typeof(LandRoad), this.Tribe) != null)
                        GameServer.ServerSend.BroadcastApplyBuild(cellsToPath[i].coordinates, typeof(LandRoad), this, f);
                }
               
            }
        }
       
        void CreateLogics()
        {
            //Idle,Move,Res,Build,Coll---Move is not needed as it will used to carry out other states
            int[] stateCounts = { 1, 0, 5, 2, 2 };

            while (stateLogics.Count < 10)
            {
                int j = random.Next(Enum.GetValues(typeof(States)).Length);
                if (stateCounts[j] == 0) continue;
                stateLogics.Enqueue((States)j );
                stateCounts[j]--;
            }
            foreach (States state in stateLogics) Console.WriteLine(state);
        }


        bool isRessourcefull()
        {
            return false;
        }
        bool isBuildingsFull()
        {
            return false;
        }
        bool isUpgradeReady()
        {
            return (isBuildingsFull() && isRessourcefull());
        }


    }
   
}
