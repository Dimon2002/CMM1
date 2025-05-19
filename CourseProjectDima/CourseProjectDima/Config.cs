using CourseProjectDima.Core.GridComponents;

namespace CourseProjectDima;

public static class Config
{
    private static readonly Func<Node2D, double, double>[] Us = 
    [
        (p, t) => Math.Pow(p.R, 1) + Math.Pow(p.Z, 1), // f = r + z
        (p, t) => Math.Pow(p.R, 2) + Math.Pow(p.Z, 2), // f = r^2 + z^2
        (p, t) => Math.Pow(p.R, 3) + Math.Pow(p.Z, 3), // f = r^3 + z^3
        (p, t) => Math.Pow(p.R - 6, 3) + Math.Pow(p.Z - 6, 3), // f = (r - 6)^3 + (z - 6)^3
        (p, t) => Math.Pow(p.R - 6, 4) + Math.Pow(p.Z - 6, 4), // f = (r - 6)^4 + (z - 6)^4
        (p, t) => Math.Exp(p.R) + Math.Exp(p.Z), // f = exp(r) + exp(z)
        (p, t) => Math.Sin(p.R) * Math.Cos(p.Z)
    ];
    
    private static readonly Func<Node2D, double, double>[] Fs = 
    [
        (p, t) => -1 / p.R, // f = r + z
        (p, t) => -6, // f = r^2 + z^2
        (p, t) => -9 * Math.Pow(p.R, 1) -6 * Math.Pow(p.Z, 1), // f = r^3 + z^3
        (p, t) => -1 / p.R * (3 * Math.Pow(p.R - 6, 2) + 6 * p.R * (p.R - 6)) - 6 *(p.Z - 6), // f = (r - 6)^3 + (z - 6)^3
        (p, t) => -1 / p.R * (4 * Math.Pow(p.R - 6, 3) + 12 * p.R * Math.Pow(p.R - 6, 2)) - 12 * Math.Pow(p.Z - 6, 2), // f = (r - 6)^4 + (z - 6)^4
        (p, t) => -Math.Exp(p.R) * (1 + p.R) / p.R - Math.Exp(p.Z), // f = exp(r) + exp(z)
        (p, t) => Math.Cos(p.Z) * (p.R * Math.Sin(p.R) - Math.Cos(p.R)) / p.R + Math.Sin(p.R) * Math.Cos(p.Z),
    ];

    private static readonly string[] Folders =
    [
        "r+z",
        "r^2+z^2",
        "r^3+z^3",
        "(r - 6)^3 + (z - 6)^3",
        "(r - 6)^4 + (z - 6)^4",
        "exp(r) + exp(z)",
        "sin(r)cos(z)"
    ];

    private const int FuncIndex = 4;
    private const int m = 2;

    public static Func<Node2D, double, double> u = (p, t) => Us[FuncIndex](p, t);
    public static Func<Node2D, double, double> f = (p, t) => Fs[FuncIndex](p, t);
    
    public static readonly int GridSplit = 28;
    public static readonly int UNums = 100;
    public static readonly int PointsNum = 5;
    public static readonly bool PrintSurface = false;
    
    public static readonly string FolderName = Folders[FuncIndex];

    public static readonly int DegreeU = m;
    public static readonly int DegreeV = m;
}
