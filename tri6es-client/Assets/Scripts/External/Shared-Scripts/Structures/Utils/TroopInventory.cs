using System;
using System.Collections.Generic;
using System.Linq;
using Shared.DataTypes;
using UnityEngine;
using Shared.Game;

namespace Shared.Structures
{
    public class TroopInventory
    {
        public const byte WEAKNESS_DMG = 3;

        public const byte BUFFED_DMG = WEAKNESS_DMG * 2;

        public Dictionary<TroopType, int> Troops;

        public List<Tuple<TroopType, bool>> Strategy;

        public int TroopLimit;

        public TroopInventory()
        {
            TroopLimit = 20;
            Troops = new Dictionary<TroopType, int>()
            {
                { TroopType.ARCHER, 10 },
                { TroopType.KNIGHT, 0 },
                { TroopType.SPEARMAN, 0 },
                { TroopType.SCOUT, 0 },
                { TroopType.SIEGE_ENGINE, 10 },
            };

            Strategy = new List<Tuple<TroopType, bool>>();
            Strategy.Add(new Tuple<TroopType, bool>(TroopType.ARCHER, true));
            Strategy.Add(new Tuple<TroopType, bool>(TroopType.KNIGHT, true));
            Strategy.Add(new Tuple<TroopType, bool>(TroopType.SPEARMAN, true));
            Strategy.Add(new Tuple<TroopType, bool>(TroopType.SCOUT, false));
            Strategy.Add(new Tuple<TroopType, bool>(TroopType.SIEGE_ENGINE, false));
        }
        public TroopInventory(int scouts)
        {
            TroopLimit = 20;
            Troops = new Dictionary<TroopType, int>()
            {
                { TroopType.ARCHER, 0 },
                { TroopType.KNIGHT, 0 },
                { TroopType.SPEARMAN, 0 },
                { TroopType.SCOUT, 0 },
                { TroopType.SIEGE_ENGINE,0 },
            };

            Strategy = new List<Tuple<TroopType, bool>>();
            Strategy.Add(new Tuple<TroopType, bool>(TroopType.ARCHER, true));
            Strategy.Add(new Tuple<TroopType, bool>(TroopType.KNIGHT, true));
            Strategy.Add(new Tuple<TroopType, bool>(TroopType.SPEARMAN, true));
            Strategy.Add(new Tuple<TroopType, bool>(TroopType.SCOUT, false));
            Strategy.Add(new Tuple<TroopType, bool>(TroopType.SIEGE_ENGINE, false));
        }

        public TroopInventory(Dictionary<TroopType, int> troops, int troopLimit, List<Tuple<TroopType, bool>> strategy)
        {
            this.TroopLimit = troopLimit;
            this.Strategy = strategy;
            this.Troops = troops;
        }

        public void UpdateStrategy(List<Tuple<TroopType, bool>> strategy)
        {
            if (strategy.Count > 3)
                return;
            this.Strategy = strategy;
        }

        public void UpdateStrategy(int oldIndex, int newIndex)
        {
            Tuple<TroopType, bool> tpl = this.Strategy[oldIndex];
            this.Strategy.RemoveAt(oldIndex);
            this.Strategy.Insert(newIndex, tpl);
        }

        public void UpdateTroopLimit(int newValue)
        {
            this.TroopLimit = newValue;
        }

        public void AddUnit(TroopType type, int amount)
        {
            this.Troops[type] += Mathf.Min(amount, this.GetAvailableSpace());
        }

        public void RemoveUnit(TroopType type, int amount)
        {
            if (amount <= 0)
                return;
            this.Troops[type] -= Mathf.Min(amount, this.Troops[type]);
        }

        public bool MoveTroops(TroopInventory destination, TroopType troopType, int amount)
        {
            if (this.Troops[troopType] - amount >= 0 && destination.GetTroopCount() + amount <= destination.TroopLimit)
            {
                //Move troops
                this.Troops[troopType] -= amount;
                destination.Troops[troopType] += amount;
                return true;
            }
            return false;
        }

        public int GetAvailableSpace()
        {
            return TroopLimit - this.Troops.Values.Aggregate((agg, elem) => agg += elem);
        }

        public int GetTroopCount()
        {
            return this.Troops.Values.Aggregate((agg, elem) => agg += elem);
        }

        public int FightTroops(ProtectedBuilding building, Tribe tribe)
        {
            TroopInventory defender = building.TroopInventory;
            TroopInventory attacker = this;

            Tribe defTribe = GameLogic.GetTribe(building.Tribe);

            Troop attackerTroop = attacker.GetInitialTroop();
            Troop defenderTroop = defender.GetInitialTroop();

            TroopInventory attackerScouts = new TroopInventory();
            TroopInventory defenderScouts = new TroopInventory();

            //TODO: Not used?
            Troop attackerScout = attacker.GetInitialTroop();
            Troop defenderScout = defender.GetInitialTroop();

            int attamount = this.Troops[TroopType.SCOUT];
            int deffamount = defender.Troops[TroopType.SCOUT];
            attackerScouts.AddUnit(TroopType.SCOUT, attamount);
            defenderScouts.AddUnit(TroopType.SCOUT, deffamount);
            attacker.RemoveUnit(TroopType.SCOUT, attamount);
            defender.RemoveUnit(TroopType.SCOUT, deffamount);
            if (tribe.ttypes.Contains(TroopType.SCOUT))
            {
                defender.RemoveUnit(TroopType.SCOUT, attamount * 2);
            }
            else
            {
                defender.RemoveUnit(TroopType.SCOUT, attamount);
            }
            if (defTribe.ttypes.Contains(TroopType.SCOUT))
            {
                attacker.RemoveUnit(TroopType.SCOUT, deffamount * 2);
            }
            else
            {
                attacker.RemoveUnit(TroopType.SCOUT, deffamount);
            }
            bool scouts = (attamount > 0) ? true : false;

            defenderScouts.RemoveUnit(TroopType.SCOUT, Math.Min(defenderScouts.GetTroopCount(), attackerScouts.GetTroopCount()));
            attackerScouts.RemoveUnit(TroopType.SCOUT, Math.Min(defenderScouts.GetTroopCount(), attackerScouts.GetTroopCount()));
            //Didnt understand what the point is
            if (scouts)
            {
                if (attackerScouts.GetTroopCount() > 0)
                {
                    Tuple<byte, TroopInventory> toadd = new Tuple<byte, TroopInventory>(tribe.Id, defender);
                    building.scoutResults.Add(toadd);
                }
            }
            //Get Troop Power
            int battlePointsAttacker = 0;
            foreach (KeyValuePair<TroopType, int> kvp in attacker.Troops)
            {
                battlePointsAttacker += kvp.Key == TroopType.ARCHER ? kvp.Value * 4 :
                    kvp.Key == TroopType.KNIGHT ? kvp.Value * 8 :
                    kvp.Key == TroopType.SPEARMAN ? kvp.Value * 2 : kvp.Value * 0;
                if (tribe.ttypes.Contains(TroopType.ARCHER))
                {
                    battlePointsAttacker += kvp.Key == TroopType.ARCHER ? kvp.Value * 4 : kvp.Value * 0;
                }
                if (tribe.ttypes.Contains(TroopType.KNIGHT))
                {
                    battlePointsAttacker += kvp.Key == TroopType.KNIGHT ? kvp.Value * 8 : kvp.Value * 0;
                }
                if (tribe.ttypes.Contains(TroopType.SPEARMAN))
                {
                    battlePointsAttacker += kvp.Key == TroopType.SPEARMAN ? kvp.Value * 2 : kvp.Value * 0;
                }
            }
            int battlePointsdefender = 0;
            foreach (KeyValuePair<TroopType, int> kvp in defender.Troops)
            {
                battlePointsdefender += kvp.Key == TroopType.ARCHER ? kvp.Value * 4 :
                    kvp.Key == TroopType.KNIGHT ? kvp.Value * 8 :
                    kvp.Key == TroopType.SPEARMAN ? kvp.Value * 2 : kvp.Value * 0;
                if (defTribe.ttypes.Contains(TroopType.ARCHER))
                {
                    battlePointsdefender += kvp.Key == TroopType.ARCHER ? kvp.Value * 4 : kvp.Value * 0;
                }
                if (defTribe.ttypes.Contains(TroopType.KNIGHT))
                {
                    battlePointsdefender += kvp.Key == TroopType.KNIGHT ? kvp.Value * 8 : kvp.Value * 0;
                }
                if (defTribe.ttypes.Contains(TroopType.SPEARMAN))
                {
                    battlePointsdefender += kvp.Key == TroopType.SPEARMAN ? kvp.Value * 2 : kvp.Value * 0;
                }
            }
            //No fight
            if (battlePointsAttacker == 0 && battlePointsdefender == 0)
            {
                return 1;
            }

            //Fight
            if (defenderTroop != null)
            {
                while (battlePointsAttacker > 0)
                {

                    switch (defenderTroop.type)
                    {
                        case TroopType.ARCHER:
                            defender.RemoveUnit(defenderTroop.type, 1);
                            battlePointsAttacker -= 4;
                            if (defTribe.ttypes.Contains(TroopType.ARCHER))
                            {
                                battlePointsAttacker -= 4;
                            }
                            break;
                        case TroopType.SPEARMAN:
                            defender.RemoveUnit(defenderTroop.type, 1);
                            battlePointsAttacker -= 2;
                            if (defTribe.ttypes.Contains(TroopType.SPEARMAN))
                            {
                                battlePointsAttacker -= 2;
                            }
                            break;
                        case TroopType.KNIGHT:
                            defender.RemoveUnit(defenderTroop.type, 1);
                            battlePointsAttacker -= 8;
                            if (defTribe.ttypes.Contains(TroopType.KNIGHT))
                            {
                                battlePointsAttacker -= 8;
                            }
                            break;
                        case TroopType.SIEGE_ENGINE:
                            defender.RemoveUnit(defenderTroop.type, 1);
                            battlePointsAttacker -= 1;
                            break;
                        case TroopType.SCOUT:

                            break;
                        default:
                            break;
                    }
                    defenderTroop = defender.GetNextTroop(defenderTroop.type);
                    if (defenderTroop == null)
                    {
                        Console.WriteLine("defender nullandi attackerpoints: " + battlePointsAttacker);
                        break;
                    }

                }
            }
            
            if (attackerTroop != null)
            {
                while (battlePointsdefender > 0)
                {

                    switch (attackerTroop.type)
                    {
                        case TroopType.ARCHER:
                            attacker.RemoveUnit(attackerTroop.type, 1);
                            battlePointsdefender -= 4;
                            if (tribe.ttypes.Contains(TroopType.ARCHER))
                            {
                                battlePointsdefender -= 4;
                            }
                            break;
                        case TroopType.SPEARMAN:
                            attacker.RemoveUnit(attackerTroop.type, 1);
                            battlePointsdefender -= 2;
                            if (tribe.ttypes.Contains(TroopType.SPEARMAN))
                            {
                                battlePointsdefender -= 2;
                            }
                            break;
                        case TroopType.KNIGHT:
                            attacker.RemoveUnit(attackerTroop.type, 1);
                            battlePointsdefender -= 8;
                            if (tribe.ttypes.Contains(TroopType.KNIGHT))
                            {
                                battlePointsdefender -= 8;
                            }
                            break;
                        case TroopType.SIEGE_ENGINE:
                            attacker.RemoveUnit(attackerTroop.type, 1);
                            battlePointsdefender -= 1;
                            break;
                        case TroopType.SCOUT:

                            break;
                        default:
                            break;
                    }
                    attackerTroop = attacker.GetNextTroop(attackerTroop.type);
                    if (attackerTroop == null)
                    {
                        Console.WriteLine("attacker nullandi attackerpoints: " + battlePointsdefender);
                        break;
                    }

                }
            }
           

            //Results
            if (battlePointsAttacker == battlePointsdefender)
            {
                return (int)Feedback.BattleLog.BattleScore.Tie;
            }
            else if (battlePointsAttacker > battlePointsdefender)
            {
                return (int)Feedback.BattleLog.BattleScore.attackerWon;
            }
            else
            {
                return (int)Feedback.BattleLog.BattleScore.defenderWon;
            }
            #region TEST
            Console.WriteLine("attackerpoints: " + battlePointsAttacker);
            Console.WriteLine("defpoints: " + battlePointsdefender);


            while (attackerTroop != null && defenderTroop != null)
            {
                //Console.WriteLine("Fight öncesi"+attackerTroop.ToString());
                //Console.WriteLine("Fight öncesi" + defenderTroop.ToString());
                if (attackerTroop == null) continue;
                attackerTroop.Fight(defenderTroop);

                if (attackerTroop.health <= 0)
                {
                    attacker.RemoveUnit(attackerTroop.type, 1);
                    attackerTroop = attacker.GetNextTroop(attackerTroop.type);



                }
                if (defenderTroop.health <= 0)
                {
                    defender.RemoveUnit(defenderTroop.type, 1);
                    defenderTroop = defender.GetNextTroop(defenderTroop.type);

                }
                try
                {
                    if (attackerTroop.type == TroopType.SCOUT)
                    {
                        attackerTroop = attacker.GetNextTroop(attackerTroop.type);
                    }
                    if (defenderTroop.type == TroopType.SCOUT)
                    {
                        defenderTroop = defender.GetNextTroop(defenderTroop.type);
                    }
                }
                catch (NullReferenceException e)
                {

                }
            }
           

            //BUG,will be fixed
            //attacker.AddUnit(TroopType.SCOUT, attackerScouts.GetTroopCount());
            //defender.AddUnit(TroopType.SCOUT, defenderScouts.GetTroopCount());

            if ((attackerTroop == null && attackerScouts.GetTroopCount() == 0) && (defenderTroop == null && defenderScouts.GetTroopCount() == 0))
            {
                //stalemate
                return 1;

            }
            else if (attackerTroop == null && attackerScouts.GetTroopCount() == 0)
            {
                //Lost
                return 2;

            }
            else
            {
                //won
                return 0;

            }

            //return attackerTroop == null && attackerScouts.GetTroopCount() == 0 ? false : true;
            #endregion
        }

        public byte AttackBuilding(ProtectedBuilding building, Tribe tribe)
        {
            int won = FightTroops(building, tribe);
            byte levels = 0;
            int health = building.Health;
            int buildingLevel = (int)building.Level;

            //Only seiege engines can destroy buildings
            //Troop[] _siegeEngines = new Troop[this.Troops[TroopType.SIEGE_ENGINE]];

            if (won == 0)
            {
                Console.WriteLine("savs oldu");
                while (this.Troops[TroopType.SIEGE_ENGINE] > 0)
                {

                    if (tribe.ttypes.Contains(TroopType.SIEGE_ENGINE))
                    {
                        health -= (int)BUFFED_DMG;
                    }
                    else
                    {
                        health -= (int)WEAKNESS_DMG;
                    }
                    this.RemoveUnit(TroopType.SIEGE_ENGINE, 1);


                    if (health <= 0)
                    {
                        levels++;
                        if (buildingLevel == 0) break;
                        buildingLevel--;
                        health = building.MaxHealths[buildingLevel]; //make it possible to reduce multiple levels
                    }

                }
                //Troop troop = GetInitialTroop();
                //for (int i =0; i < _siegeEngines.Length; i++)
                //{
                //    damage += troop.type == TroopType.SIEGE_ENGINE ? (int)WEAKNESS_DMG : (int)1;
                //    health -= troop.type == TroopType.SIEGE_ENGINE ? (int)WEAKNESS_DMG : (int)1;
                //    this.RemoveUnit(troop.type, 1);
                //    troop = GetNextTroop(troop.type);

                //    if (health <= 0)
                //    {
                //        levels++;
                //        buildingLevel--;
                //        health = building.MaxHealths[buildingLevel]; //make it possible to reduce multiple levels
                //    }
                //}
                //while (troop != null)
                //{
                //    if (troop.type != TroopType.SIEGE_ENGINE) troop = GetNextTroop(troop.type); 

                //    damage += troop.type == TroopType.SIEGE_ENGINE ? (int)WEAKNESS_DMG : (int)1;
                //    health -= troop.type == TroopType.SIEGE_ENGINE ? (int)WEAKNESS_DMG : (int)1;
                //    this.RemoveUnit(troop.type, 1);
                //    troop = GetNextTroop(troop.type);

                //    //if (troop.type == TroopType.SCOUT) {
                //    //    this.RemoveUnit(troop.type, 1);
                //    //    troop = GetNextTroop(troop.type);
                //    //}

                //    //damage += troop.type == TroopType.SIEGE_ENGINE ? (int)WEAKNESS_DMG : (int)1;
                //    //health -= troop.type == TroopType.SIEGE_ENGINE ? (int)WEAKNESS_DMG : (int) 1;
                //    //this.RemoveUnit(troop.type, 1);
                //    ////this doesnt include Siege Engine
                //    //troop = GetNextTroop(troop.type);


                //    if (health <= 0)
                //    {
                //        levels++;
                //        buildingLevel--;
                //        health = building.MaxHealths[buildingLevel]; //make it possible to reduce multiple levels
                //    }

                //}
            }
        
            return levels;
        }

        protected Troop GetInitialTroop()
        {
            for (int i = 0; i < this.Strategy.Count; i++)
            {
                if (this.Troops[this.Strategy[i].Item1] > 0)
                    return new Troop(this.Strategy[i].Item1);
            }
            return null;
        }

        protected Troop GetNextTroop(TroopType currentType)
        {
            try
            {
                //locate where currenttype on the strategy is
                int foundIndex = this.Strategy.FindIndex(elem => elem.Item1 == currentType);
                //Check wheter we have the sametype in our troops
                if (this.Troops[this.Strategy[foundIndex].Item1] > 0)
                {
                    //if yes then return this sametype as a new troop
                    return new Troop(this.Strategy[foundIndex].Item1);
                }
                //if dont have this kind of trroptype then check other trooptypes
                foundIndex++;
                //Console.WriteLine("foundindex : "+this.Strategy.ElementAt(foundIndex));
                while (foundIndex < this.Strategy.Count)
                {
                    if (Strategy[foundIndex].Item2 == true && this.Troops[this.Strategy[foundIndex].Item1] > 0)
                    {
                        //if this trroptype can be used in battle, and if we have them in our troplist return the new typeof Troop
                        //Console.WriteLine("dönen : " + this.Strategy[i].Item1);
                        return new Troop(this.Strategy[foundIndex].Item1);
                    }
                    foundIndex++;
                }
            }
            catch (Exception e)
            {
                //return GetInitialTroop();
                throw new Exception(e.Message);
            }
            //if dont find any then return null;
            return null;
            try
            {
                int foundIndex = this.Strategy.FindIndex(elem => elem.Item1 == currentType);

                if (Strategy[foundIndex].Item2 == true)
                {
                    for (int i = foundIndex + 1; i < this.Strategy.Count; i++)
                    {
                        if (this.Strategy[i].Item2 == false)
                            break;

                        if (this.Troops[this.Strategy[i].Item1] > 0)
                        {
                            return new Troop(this.Strategy[i].Item1);
                        }
                    }
                    return GetInitialTroop();
                }
                else if (Troops[currentType] > 0)
                {
                    return new Troop(currentType);
                }
                else
                {
                    //TEST
                    //return GetInitialTroop();
                    return null;
                }
            }
            catch (Exception e)
            {
                //return GetInitialTroop();
                throw new Exception(e.Message);
            }
        }

        protected class Troop
        {
            public int health;
            public TroopType type;

            public Troop(TroopType type)
            {
                this.type = type;
                this.health = 12;
            }
            public override string ToString()
            {
                return type.ToString() + " " + health;
            }

            public void Fight(Troop other)
            {
                //From what i understand, same trooptypes are at stalemate. Knights always win. Spearmen always lose. Archer wins against spearmen
                if (this.type == other.type)
                {
                    // tie
                    this.health -= 1;
                    other.health -= 1;
                    return;
                }

                switch (other.type)
                {
                    case TroopType.SIEGE_ENGINE:
                        // we win
                        this.health -= 1;
                        other.health -= WEAKNESS_DMG;
                        break;
                    case TroopType.KNIGHT:
                        // we lose
                        this.health -= WEAKNESS_DMG;
                        other.health -= 1;
                        break;
                    case TroopType.ARCHER:
                        if (this.type == TroopType.SPEARMAN)
                        {
                            // we lose
                            this.health -= WEAKNESS_DMG;
                            other.health -= 1;
                            break;
                        }
                        else
                        {
                            // we win
                            this.health -= 1;
                            other.health -= WEAKNESS_DMG;
                            break;
                        }
                    case TroopType.SPEARMAN:
                        // we win
                        this.health -= 1;
                        other.health -= WEAKNESS_DMG;
                        break;
                    default:
                        Console.WriteLine("Undefined fight in >TroopInventory->Fight");
                        break;

                }
                return;
                if (
                    other.type == TroopType.SIEGE_ENGINE
                    || (this.type == TroopType.ARCHER && other.type == TroopType.SPEARMAN)
                    || (this.type == TroopType.KNIGHT && other.type == TroopType.ARCHER)
                    || (this.type == TroopType.SPEARMAN && other.type == TroopType.ARCHER)
                )
                {
                    // we win
                    this.health -= 1;
                    other.health -= WEAKNESS_DMG;
                }
                else if (
                    this.type == TroopType.SIEGE_ENGINE
                    || (this.type == TroopType.SPEARMAN && other.type == TroopType.ARCHER)
                    || (this.type == TroopType.ARCHER && other.type == TroopType.KNIGHT)
                    || (this.type == TroopType.ARCHER && other.type == TroopType.SPEARMAN)
                )
                {
                    // we lose
                    this.health -= WEAKNESS_DMG;
                    other.health -= 1;
                }
                else
                {
                    // tie
                    this.health -= 1;
                    other.health -= 1;
                }
            }
        }
    }
}
