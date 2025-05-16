using Shared.HexGrid;

namespace Shared.Structures
{
    public abstract class ProgressBuilding : InventoryBuilding
    {
        public int MaxProgress { get { return MaxProgresses[Level - 1]; } }
        public abstract int[] MaxProgresses { get; }

        public int Progress;


        public ProgressBuilding() : base()
        {
            Progress = 0;
        }

        public ProgressBuilding(
            HexCell Cell,
            byte Tribe,
            byte Level,
            BuildingInventory Inventory,
            int Progress
            ) : base(Cell, Tribe, Level, Inventory)
        {
            this.Progress = Progress;
        }

        public abstract void OnMaxProgress();

        public override void DoTick()
        {
            base.DoTick();
            if (Progress > 0)
            {
                //test
                Progress++;
                Progress+=300;
            }
            if (Progress >= MaxProgress)
            {
                OnMaxProgress();
                Progress = 0;
            }
        }
    }
}
