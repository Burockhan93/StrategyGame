using System;
using System.ComponentModel;
using System.Linq;
using Shared.HexGrid;

namespace Shared.Structures
{
    public abstract class Structure
    {
        public HexCell Cell;

        public Structure()
        {
            this.Cell = null;
        }

        public Structure(HexCell Cell)
        {
            this.Cell = Cell;
        }

        public abstract void DoTick();

        public virtual bool IsPlaceableAt(HexCell cell)
        {
            return false;
        }

        public string GetFriendlyName()
        {
            return GetFriendlyName(GetType());
        }

        public static string GetFriendlyName(Type structure)
        {
            DescriptionAttribute attribute = structure.GetCustomAttributes(typeof(DescriptionAttribute), false).SingleOrDefault() as DescriptionAttribute;

            // name is attribute or raw type name as default
            return attribute != null ? attribute.Description : structure.ToString().Split('.').Last();;
        }
    }
}
