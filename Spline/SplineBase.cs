using Spline.BasisInfos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Spline;

public abstract class SplineBase
{
    protected virtual Func<double, double> GetBasisFunction(BasisInfo _info)
    {
        throw new NotImplementedException();
    }
    public struct Point3d
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public Point3d(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Point3d(IEnumerable<double> xyz)
        {
            X = xyz.ElementAt(0);
            Y = xyz.ElementAt(1);
            Z = xyz.ElementAt(2);
        }

        public static Point3d operator *(Point3d pt, double val)
        {
            return new Point3d(pt.X * val, pt.Y * val, pt.Z * val);
        }

        public static Point3d operator *(double val, Point3d pt)
        {
            return new Point3d(pt.X * val, pt.Y * val, pt.Z * val);
        }
        public static Point3d operator /(Point3d pt, double val)
        {
            return new Point3d(pt.X / val, pt.Y / val, pt.Z / val);
        }

        public static Point3d operator +(Point3d pt1, Point3d pt2)
        {
            return new Point3d(pt1.X + pt2.X, pt1.Y + pt2.Y, pt1.Z + pt2.Z);
        }

        public override readonly string ToString() => $"X:{X}, Y:{Y}, Z:{Z}";
    }
}
