using System.Linq;
using System.Reflection;
using System.ComponentModel;

namespace Shared.DataTypes
{
    public enum CollectableType
    {
        [Description("Loot")]
        LOOT,
        [Description("Buff")]
        WOODBUFF,
        STONEBUFF,
        COALBUFF,
        WHEATBUFF,
        COWBUFF,
        SEBUFF,
        SCOUTBUFF,
        SPEARMANBUFF,
        ARCHERBUFF,
        KNIGHTBUFF,

    }

    public static class CollectableTypeExtension
    {
        public static string ToFriendlyString(this CollectableType collectable)
        {
            string rawName = collectable.ToString();
            // get description attribute
            FieldInfo field = collectable.GetType().GetField(rawName);
            DescriptionAttribute attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false).SingleOrDefault() as DescriptionAttribute;

            // name is attribute or raw enum name as default
            return attribute != null ? attribute.Description : rawName;
        }
    }

}
