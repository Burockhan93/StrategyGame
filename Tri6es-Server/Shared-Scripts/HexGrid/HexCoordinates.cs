using UnityEngine;


namespace Shared.HexGrid
{
    [System.Serializable]
    public struct HexCoordinates
    {
        private int x, z;

        public int X
        {
            get
            {
                return x;
            }
        }

        public int Y
        {
            get
            {
                return -X - Z;
            }
        }

        public int Z
        {
            get
            {
                return z;
            }
        }

        public HexCoordinates(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        public HexCoordinates InDirection(HexDirection direction)
        {
            if((int)direction % 3 == 0)
            {
                return new HexCoordinates(x, z + (((int)direction % 2 == 0) ? 1 : -1));
            }
            else if ((int)direction % 3 == 1)
            {
                return new HexCoordinates(x + (((int)direction % 2 == 0) ? 1 : -1), z);
            }
            else 
            {
                return new HexCoordinates(x + (((int)direction % 2 == 0) ? 1 : -1), z + (((int)direction % 2 == 0) ? -1 : 1));
            }
        }

        public int ToOffsetX()
        {
            return x + z / 2;
        }

        public int ToOffsetZ()
        {
            return z;
        }

        public int ToChunkX()
        {
            return ToOffsetX() / HexMetrics.chunkSizeX;
        }

        public int ToChunkZ()
        {
            return ToOffsetZ() / HexMetrics.chunkSizeZ;
        }


        public static HexCoordinates FromOffsetCoordinates(int x, int z)
        {
            return new HexCoordinates(x - z / 2, z);
        }
       
        public static HexCoordinates FromPosition(Vector3 position)
        {
            float x = position.x / (HexMetrics.innerRadius * 2f);
            float y = -x;
            float offset = position.z / (HexMetrics.outerRadius * 3f);
            x -= offset;
            y -= offset;
            int iX = Mathf.RoundToInt(x);
            int iY = Mathf.RoundToInt(y);
            int iZ = Mathf.RoundToInt(-x - y);

            if (iX + iY + iZ != 0)
            {
                float dX = Mathf.Abs(x - iX);
                float dY = Mathf.Abs(y - iY);
                float dZ = Mathf.Abs(-x - y - iZ);

                if (dX > dY && dX > dZ)
                {
                    iX = -iY - iZ;
                }
                else if (dZ > dY)
                {
                    iZ = -iX - iY;
                }
            }

            return new HexCoordinates(iX, iZ);
        }




        public override string ToString()
        {
            return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
        }

        public string ToStringOnSeperateLines()
        {
            return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
        }

        public static bool operator== (HexCoordinates c1, HexCoordinates c2)
        {
            if (c1.X == c2.X && c1.Z == c2.Z)
                return true;
            return false;
        }

        public static bool operator!= (HexCoordinates c1, HexCoordinates c2)
        {
            return !(c1 == c2);
        }
       
        public static HexCoordinates operator++ (HexCoordinates c1)
        {
            //if Z < cell count in z ax and x < cell count in x axis/2. Will be corrected
            return new HexCoordinates (c1.X+1, c1.Z+1);
        }

        public static int calcDistance(HexCoordinates source, HexCoordinates destination) {
            int aX1 = source.X;
            int aY1 = source.Y;
            int aX2 = destination.X;
            int aY2 = destination.Y;
            int dx = aX2 - aX1;
            int dy = aY2 - aY1;
            int x = Mathf.Abs(dx);
            int y = Mathf.Abs(dy);
            // special case if we start on an odd row or if we move into negative x direction
            if ((dx < 0)^((aY1&1)==1))
                x = Mathf.Max(0, x - (y + 1) / 2);
            else
                x = Mathf.Max(0, x - (y) / 2);
            return x + y;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}

