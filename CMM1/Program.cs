using CMM1;
using CourseProjectDima;
using CourseProjectDima.Core.GridComponents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SharpMath.EquationsSystem.Solver;
using SharpMath.Geometry._2D;
using SharpMath.Geometry.Splitting;
using SharpMath.Splines;
using System.Globalization;
using Spline;
using Spline.Utils;
using Element = SharpMath.FiniteElement._2D.Element;

Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

void ConfigureServices(IServiceCollection services)
{
    IConfiguration configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    services.AddScoped<GaussZeidelConfig>(provider =>
    {
        provider.GetService<IConfiguration>();
        var gaussZeidelConfig = configuration
            .GetSection("CMM1")
            .GetSection("GaussZeidel")
            .Get<GaussZeidelConfig>();

        return gaussZeidelConfig!;
    });

    services.AddSingleton(configuration);

    services.AddTransient<ISplineCreator<ISpline2D<Point>, Point, Element>, SmoothingSpline2DCreator>();
    services.AddTransient<SmoothingSpline2DCreator>();

    services.AddScoped<GaussZeidelSolver>();


    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .Enrich.FromLogContext()
        .CreateLogger();
    services.AddLogging(loggingBuilder =>
        loggingBuilder.AddSerilog(dispose: true));
}

void RunTest()
{
    var services = new ServiceCollection();
    ConfigureServices(services);

    var tests = new Tests();

    var inputPoints = tests.GetPoints(Config.PointsNum - 1);

    var funcValues = tests.GetFuncValues(inputPoints);
    //funcValues[12].Value = 1;

    int pointXNum = Config.PointsNum;
    int pointYNum = Config.PointsNum;

    KnotSet uKnot = KnotSet.CreateToUniform(Config.DegreeU, pointXNum);
    KnotSet vKnot = KnotSet.CreateToUniform(Config.DegreeV, pointYNum);

    double[][][] ptGrid = new double[pointXNum][][];
    for (var i = 0; i < pointXNum; i++)
    {
        ptGrid[i] = new double[pointYNum][];
        for (var j = 0; j < pointYNum; j++)
        {
            ptGrid[i][j] = new double[3];
            ptGrid[i][j][0] = inputPoints[j * pointXNum + i].X;
            ptGrid[i][j][1] = inputPoints[j * pointXNum + i].Y;
            ptGrid[i][j][2] = funcValues[j * pointXNum + i].Value;
        }
    }

    var surface = new BSplineSurface(ptGrid, uKnot, vKnot, Config.DegreeU, Config.DegreeV);

    surface = surface
        .CreateInterpolatingSurface();

    var us = new double[Config.UNums + 1];
    Point[] points3D = new Point[us.Length * us.Length];
    Point[] points2D = new Point[us.Length];

    for (int i = 0; i < us.Length; i++)
    {
        us[i] = (1d / Config.UNums) * i;
    }

    for (int i = 0; i < us.Length; i++)
    {
        for (int j = 0; j < us.Length; j++)
        {
            var r = surface.ParameterAt(us[i], us[j]);
            points3D[i * us.Length + j] = new Point(r[0], r[1]);
        }
    }

    for (int i = 0; i < us.Length; i++)
    {
        var r = surface.ParameterAt(us[i], us[i]);
        points2D[i] = new Point(r[0], r[1]);
    }

    var femSolution2D = tests.GetFuncValues(points2D);
    var femSolution3D = tests.GetFuncValues(points3D);

    var femPath2D = "../../../../CMM1.View/" + Config.FolderName + "/dataFEM2D.txt";
    var femPath3D = "../../../../CMM1.View/" + Config.FolderName + "/dataFEM3D.txt";
    var splinePath2D = "../../../../CMM1.View/" + Config.FolderName + "/dataSpline2D.txt";
    var splinePath3D = "../../../../CMM1.View/" + Config.FolderName + "/dataSpline3D.txt";
    var truePath2D = "../../../../CMM1.View/" + Config.FolderName + "/dataTrue2D.txt";
    var truePath3D = "../../../../CMM1.View/" + Config.FolderName + "/dataTrue3D.txt";
    var pointsPath = "../../../../CMM1.View/" + Config.FolderName + "/points.txt";

    Directory.CreateDirectory("../../../../CMM1.View/" + Config.FolderName);

    using var writerFEM2D = new StreamWriter(femPath2D);
    using var writerFEM3D = new StreamWriter(femPath3D);

    using var writerSpline2D = new StreamWriter(splinePath2D);
    using var writerSpline3D = new StreamWriter(splinePath3D);

    using var writerTrue2D = new StreamWriter(truePath2D);
    using var writerTrue3D = new StreamWriter(truePath3D);

    using var writerPoints = new StreamWriter(pointsPath);
    using var configWriter = new StreamWriter("../../../../CMM1.View/config.txt");
    configWriter.Write(Config.FolderName);

    Console.WriteLine("FEM solution");
    for (var i = 0; i < points2D.Length; i++)
    {
        var point = points2D[i];
        writerFEM2D.WriteLine($"{point.X:F8} {point.Y:F8} {femSolution2D[i].Value:E8}");
    }
    for (var i = 0; i < points3D.Length; i++)
    {
        var point = points3D[i];
        writerFEM3D.WriteLine($"{point.X:F8} {point.Y:F8} {femSolution3D[i].Value:E8}");
    }

    Console.WriteLine("Spline solution");

    for (int i = 0; i < us.Length; i++)
    {
        for (int j = 0; j < us.Length; j++)
        {
            var r = surface.ParameterAt(us[i], us[j]);
            writerSpline3D.WriteLine($"{r[0]:F8} {r[1]:F8} {r[2]:E8}");
        }
    }
    for (var i = 0; i < us.Length; i++)
    {
        var r = surface.ParameterAt(us[i], us[i]);
        writerSpline2D.WriteLine($"{r[0]:F8} {r[1]:F8} {r[2]:E8}");
    }

    Console.WriteLine("True solution");
    
    Func<Node2D, double, double> u = Config.u;
    foreach (var point in points2D)
    {
        writerTrue2D.WriteLine($"{point.X:F8} {point.Y:F8} {u(new Node2D(point.X, point.Y), 1):E8}");
    }
    foreach (var point in points3D)
    {
        writerTrue3D.WriteLine($"{point.X:F8} {point.Y:F8} {u(new Node2D(point.X, point.Y), 1):E8}");
    }

    for (var i = 0; i < pointXNum; i++)
    {
        writerPoints.WriteLine($"{ptGrid[i][i][0]:F8} {ptGrid[i][i][2]:F8}");
    }
}

RunTest();