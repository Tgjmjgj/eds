using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using BINT = System.Numerics.BigInteger;

namespace GOST_EDS
{
    static class EllipticCurve_EDS
    {
        private static BINT p = new BINT();
        private static BINT a = new BINT();
        private static BINT b = new BINT();
        private static BINT r = new BINT();
        
        public class EllipticCurve_Point
        {
            BINT x;
            BINT y;

            public BINT X
            {
                get
                {   return this.x;   }
                set
                {   this.x = value;  }
            }

            public BINT Y
            {
                get
                {   return this.y;   }
                set
                {   this.y = value;  }
            }

            public EllipticCurve_Point()
            {
                this.X = new BINT();
                this.Y = new BINT();
            }

            public EllipticCurve_Point(BINT xx, BINT yy)
            {
                this.X = xx;
                this.Y = yy;
            }

            public static bool operator ==(EllipticCurve_Point first, EllipticCurve_Point second)
            {
                return first.X == second.X && first.Y == second.Y;
            }

            public static bool operator !=(EllipticCurve_Point first, EllipticCurve_Point second)
            {
                return first.X != second.X && first.Y != second.Y; 
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public static EllipticCurve_Point operator +(EllipticCurve_Point first, EllipticCurve_Point second)
            {
                var result = new EllipticCurve_Point();
                BINT dx, dy, buf;
                if (first == second)
                {
                    dx = 2 * first.Y;
                    dy = 3 * first.X * first.X + EllipticCurve_EDS.A;
                }
                else
                {
                    dx = second.x - first.x;
                    dy = second.y - first.y;
                }
                if (dx < 0)
                    dx += EllipticCurve_EDS.P;
                if (dy < 0)
                    dy += EllipticCurve_EDS.P;
                buf = (dy * EllipticCurve_EDS.GetInverse(dx, EllipticCurve_EDS.P)) % EllipticCurve_EDS.P;
                if (buf < 0)
                    buf += EllipticCurve_EDS.P;
                result.x = (BINT.Pow(buf, 2) - first.x - second.x) % EllipticCurve_EDS.P;
                result.y = (buf * (first.x - result.x) - first.y) % EllipticCurve_EDS.P;
                if (result.X < 0)
                    result.x += EllipticCurve_EDS.P;
                if (result.y < 0)
                    result.y += EllipticCurve_EDS.P;
                return result;
            }

            public static EllipticCurve_Point operator *(EllipticCurve_Point point, BINT mult)
            {
                EllipticCurve_Point buf = point;
                mult--;
                while (mult != BINT.Zero)
                {
                    if ((mult % 2) != 0)
                    {
                        if ((buf.X == point.X) || (buf.Y == point.Y))
                            buf += buf;
                        else
                            buf = buf + point;
                        mult--;
                    }
                    mult /= 2;
                    point += point;
                }
                return buf;
            }
        }

        private static EllipticCurve_Point g = new EllipticCurve_Point();

        public static BINT P
        {
            get
            {   return p;  }
            set
            {  p = value;  }
        }

        public static BINT A
        {
            get
            {   return a;   }
            set
            {   a = value;  }
        }

        public static BINT B
        {
            get
            {   return b;   }
            set
            {   b = value;  }
        }

        public static BINT R
        {
            get
            {   return r;   }
            set
            {   r = value;  }
        }

        internal static EllipticCurve_Point G
        {
            get
            {   return g;   }
            set
            {   g = value;  }
        }

        public static bool CreateKeys(int BitSize, out BINT privateKey, out EllipticCurve_Point publicKey)
        {
            bool ret = true;
            try
            {
                privateKey = new BINT();
                do
                    privateKey = RandBINT(BitSize);
                //p_key = BINT.Parse("4837328997403298421402350564916840116247638770300050988185");
                while ((privateKey < BINT.Zero) || (privateKey > R));
                publicKey = G * privateKey;
            }
            catch
            {
                ret = false;
                privateKey = BINT.MinusOne;
                publicKey = new EllipticCurve_Point(-1, -1);
            }
            return ret;
        }

        public static BINT ModSqrt(BINT a, BINT q)
        {
            BINT generRnd = new BINT();
            do
                generRnd = RandBINT(255);
                //generRnd = BINT.Parse("51391103986756412773428879179371141425536895036213683172805728147853358774879");
            while (BINT.ModPow(generRnd, (q - 1) / 2, q) == 1);
            BINT s = 0;
            BINT t = q - 1;
            while ((t & 1) != 1)
            {
                s++;
                t = t >> 1;
            }
            BINT InvA = GetInverse(a, q);
            BINT c = BINT.ModPow(generRnd, t, q);
            BINT r = BINT.ModPow(a, ((t + 1) / 2), q);
            BINT d = new BINT();
            for (int i = 1; i < s; i++)
            {
                BINT temp = 2;
                temp = BINT.ModPow(temp, (s - i - 1), q);
                d = BINT.ModPow(BINT.ModPow(r, 2, q) * InvA, temp, q);
                if (d == (q - 1))
                    r = (r * c) % q;
                c = BINT.ModPow(c, 2, q);
            }
            return r;
        }

        public static string GenerateEDS(byte[] hash, BINT d)
        {
            BINT alpha = BINT.Parse(DecStringFromByteArray(hash));
            BINT e = alpha % R;
            if (e == BINT.Zero)
                e++;
            BINT k = new BINT();
            var C = new EllipticCurve_Point();
            BINT m = new BINT();
            BINT s = new BINT();
            do
            {
                do
                    k = RandBINT(Length(R));
                    //k = BINT.Parse("3715661474493826267033674254660166700383039479993734180117");
                while ((k < BINT.Zero) || (k > R));
                C = G * k;
                m = C.X % R;
                s = ((m * d) + (k * e)) % R;
            }
            while ((m == 0) || (s == 0));
            int midl = Length(R) / 4;
            return AddLenght(m.ToString("X"), midl) + AddLenght(s.ToString("X"), midl);
        }

        public static bool VerifiedEDS(byte[] hash, string eds, EllipticCurve_Point Q)
        {
            string Right = eds.Substring(0, Length(R) / 4);
            string Left = eds.Substring(Length(R) / 4, Length(R) / 4);
            BINT m = BINT.Parse(DecStringFromHexString(Right));
            BINT s = BINT.Parse(DecStringFromHexString(Left));
            if ((m < 1) || (m > (R - 1)) || (s < 1) || (s > (R - 1)))
                return false;
            BINT alpha = BINT.Parse(DecStringFromByteArray(hash));
            BINT e = alpha % R;
            if (e == BINT.Zero)
                e = 1;
            BINT v = GetInverse(e, R);
            BINT z1 = (s * v) % R;
            BINT z2 = R + ((-(m * v)) % R);
            EllipticCurve_Point C = G * z1 + Q * z2;
            BINT N = C.X % R;
            if (N == m)
                return true;
            else
                return false;
        }

        private static string AddLenght(string input, int size)
        {
            if (input.Length != size)
            {
                do
                    if (input.Length < size)
                        input = "0" + input;
                    else
                        input = input.Substring(1);
                while (input.Length != size);
            }
            return input;
        }

        private static BINT RandBINT(int len)
        {
            int b = len;
            int ost = b % 8;
            int b_count = (b / 8) + (ost == 0 ? 0 : 1);
            if (b_count == 0)
                return BINT.Zero;
            byte ml = (byte)Math.Pow(2, (b - 1) % 8);
            Random rnd = new Random();
            for (int i = ((b - 1) % 8) - 1; i >= 0; i--)
                ml += (byte)(Math.Pow(2, i) * rnd.Next(0, 2));
            byte[] arr = new byte[b_count];
            arr[0] = ml;
            for (int i = 1; i < b_count; i++)
                arr[i] = (byte)rnd.Next(0, 256);
            return BINT.Parse(DecStringFromByteArray(arr));
        }

        public static BINT GetInverse(BINT a, BINT m)
        {
            BINT x, y;
            BINT g = GCD(a, m, out x, out y);
            if (g != 1)
                throw new ArgumentException();
            return (x % m + m) % m;
        }

        private static BINT GCD(BINT a, BINT b, out BINT x, out BINT y)
        {
            if (a == 0)
            {
                x = 0;
                y = 1;
                return b;
            }
            BINT x1, y1;
            BINT d = GCD(b % a, a, out x1, out y1);
            x = y1 - (b / a) * x1;
            y = x1;
            return d;
        }

        private static int Length(BINT a)
        {
            byte[] arr = a.ToByteArray();
            int count = (arr.Length - 1) * 8;
            while ( arr[arr.Length - 1] != 0)
            {
                count++;
                arr[arr.Length - 1] /= 2;
            }
            return count;
        }
        
        public static string DecStringFromHexString(string hex)
        {
            Dictionary<char, int> dic = new Dictionary<char, int>() { { '0', 0 }, { '1', 1 }, { '2', 2 },
                { '3', 3 }, { '4', 4 }, { '5', 5 }, { '6', 6 }, { '7', 7 }, { '8', 8 }, { '9', 9 },
                { 'A', 10 }, { 'B', 11 }, { 'C', 12 }, { 'D', 13 }, { 'E', 14 }, { 'F', 15 } };
            BINT buf = BINT.Zero;
            hex = hex.ToUpper();
            for (int i = 0; i < hex.Length; i++)
                buf += BINT.Multiply(BINT.Pow(16, i), dic[hex[hex.Length - 1 - i]]);
            return buf.ToString();
        }

        public static string DecStringFromByteArray(byte[] bts)
        {
            BINT buf = BINT.Zero;
            for (int i = 0; i < bts.Length; i++)
                buf += BINT.Multiply(BINT.Pow(256, i), bts[bts.Length - 1 - i]);
            return buf.ToString();
        }

        public static string HexStringFromByteArray(byte[] b)
        {
            string ret = "";
            for (int i = 0; i < b.Length; i++)
            {
                ret += b[i].ToString("X");
            }
            return ret;
        }
    }
}
