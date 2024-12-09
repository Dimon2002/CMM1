using CourseProjectDima.Core.GridComponents;
using CourseProjectDima.TwoDimensional.Parameters;

namespace CourseProjectDima;

public static class Config
{
    // f = r + z
    //public static Func<Node2D, double, double> u = (p, t) => Math.Pow(p.R, 1) + Math.Pow(p.Z, 1);
    //public static Func<Node2D, double, double> f = (p, t) => -1 / p.R;

    // f = r^2 + z^2
    //public static Func<Node2D, double, double> u = (p, t) => Math.Pow(p.R, 2) + Math.Pow(p.Z, 2);
    //public static Func<Node2D, double, double> f = (p, t) => -6;

    // f = exp(r) + exp(z)
    public static Func<Node2D, double, double> u = (p, t) => Math.Exp(p.R) + Math.Exp(p.Z);
    public static Func<Node2D, double, double> f = (p, t) => -Math.Exp(p.R) * (1 + p.R) / p.R - Math.Exp(p.Z);
}
