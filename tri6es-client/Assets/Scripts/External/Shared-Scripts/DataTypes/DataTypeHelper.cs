
namespace Shared.DataTypes
{
    internal static class DataTypeHelper
    {
        internal static uint GetSubBits(this uint data, int startingBit, int numberOfBits)
        {
            uint buffer = data;
            return (buffer << (32 - startingBit - numberOfBits)) >> (32 - numberOfBits);
        }

        internal static uint SetSubBits(this uint data, uint value, int starting, int numberOfBits)
        {
            uint mask = 0;
            for (int i = 0; i <= numberOfBits; i++)
            {
                mask <<= 1;
                mask += 1;
            }
            mask <<= starting;
            return (data & ~(mask)) + (value << starting);
        }
    }
}
