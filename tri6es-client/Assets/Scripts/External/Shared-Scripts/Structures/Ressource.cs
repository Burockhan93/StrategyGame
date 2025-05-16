using Shared.DataTypes;
using Shared.HexGrid;
using System.Collections.Generic;

namespace Shared.Structures
{
    public abstract class Ressource : Structure
    {
        public int Progress;
        public int i;

        public abstract int MaxProgress { get; }
        public abstract RessourceType ressourceType { get; }
        public abstract int harvestReduction { get; }
        public abstract Dictionary<string, int> Weather { get; }

        public Ressource() : base()
        {
            this.Progress = MaxProgress;
        }

        public Ressource(HexCell cell) : base(cell)
        {
            this.Progress = MaxProgress;
        }

        public Ressource(HexCell Cell, int Progress) : base(Cell)
        {
            this.Progress = Progress;
        }

        public virtual bool Harvestable()
        {
            if (Progress - harvestReduction >= 0)
            {
                return true;
            }
            return false;
        }

        public void Harvest()
        {
            Progress -= harvestReduction;
            return;
        }

        public override void DoTick()
        {
            if (Progress < MaxProgress)
            {
                Progress++;
            }
        }
        public void DoDawnTick()
        {

            if (Progress < MaxProgress)
            {
                Progress++;
                Progress++;
            }
        }

        public void DoDuskTick()
        {
            if (Progress < MaxProgress)
            {
                i++;
                if (i == 2)
                {
                    Progress++;
                    i = 0;
                }
            }
        }
        public void DoNightTick()
        {
            if (Progress < MaxProgress)
            {
                i++;
                if (i == 4)
                {
                    Progress++;
                    i = 0;
                }
            }
        }
        public void DoSnowTick()
        {

            // weather
            if (Progress < MaxProgress)
            {
                if (Progress % (100 / Weather["snow"]) == 0)
                {
                    Progress++;
                }
            }
        }
        public void DoRainTick()
        {

            // weather
            if (Progress < MaxProgress)
            {
                if (Progress % (100 / Weather["rain"]) == 0)
                {
                    Progress++;
                }
            }
        }
        public void DoWindTick()
        {

            // weather
            if (Progress < MaxProgress)
            {
                if (Progress % (100 / Weather["wind"]) == 0)
                {
                    Progress++;
                }
            }
        }
        public void DoSunTick()
        {

            // weather
            if (Progress < MaxProgress)
            {
                if (Progress % (100 / Weather["sun"]) == 0)
                {
                    //Debug.Log("SunTick: "+ Progress);
                    Progress++;
                }
            }
        }

        public virtual bool ManuallyHarvestable()
        {
            return true;
            /*
            if (Progress == MaxProgress)
                return true;
            else
                return false;
                */
        }

        public virtual int HarvestManually()
        {
            Progress = 0;
            return 1;
        }

        public virtual int HarvestManuallyWithBoost(int boost)
        {
            Progress = 0;
            return 1 + boost;
        }
    }
}
