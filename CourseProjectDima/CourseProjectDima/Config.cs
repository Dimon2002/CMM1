using CourseProjectDima.Core.GridComponents;
using CourseProjectDima.TwoDimensional.Parameters;

namespace CourseProjectDima;

public static class Config
{
    private static readonly Func<Node2D, double, double>[] Us = 
    {
        (p, t) => Math.Pow(p.R, 1) + Math.Pow(p.Z, 1), // f = r + z
        (p, t) => Math.Pow(p.R, 2) + Math.Pow(p.Z, 2), // f = r^2 + z^2
        (p, t) => Math.Exp(p.R) + Math.Exp(p.Z), // f = exp(r) + exp(z)
        (p, t) => Math.Sin(p.R) * Math.Cos(p.Z)
    };
    
    private static Func<Node2D, double, double>[] Fs = 
    {
        (p, t) => -1 / p.R, // f = r + z
        (p, t) => -6, // f = r^2 + z^2
        (p, t) => -Math.Exp(p.R) * (1 + p.R) / p.R - Math.Exp(p.Z), // f = exp(r) + exp(z)
        (p, t) => Math.Cos(p.Z) * (p.R * Math.Sin(p.R) - Math.Cos(p.R)) / p.R + Math.Sin(p.R) * Math.Cos(p.Z),
    };

    private static readonly string[] Folders = new string[]
    {
        "r+z",
        "r^2+z^2",
        "exp(r) + exp(z)",
        "sin(r)cos(z)"
    };

    private const int FuncIndex = 3;

    public static Func<Node2D, double, double> u = (p, t) => Us[FuncIndex](p, t);
    public static Func<Node2D, double, double> f = (p, t) => Fs[FuncIndex](p, t);
    
    public static readonly int GridSplit = 5;
    public static readonly int SplineSplit = 2;
    public static readonly double Regularization = 0;
    
    public static readonly string FolderName = Folders[FuncIndex] + "/" + SplineSplit;
}
