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

    int pointXNum = Config.PointsNum;
    int pointYNum = Config.PointsNum;
    
    KnotSet uKnot = KnotSet.CreateToUniform(Config.DegreeU, pointXNum);
    KnotSet vKnot = KnotSet.CreateToUniform(Config.DegreeV, pointYNum);
    
    double[][] weights = new double[pointXNum][];
    for (var i = 0; i < pointXNum; i++)
    {
        weights[i] = new double[pointYNum];
        for (var j = 0; j < pointYNum; j++)
        {
            weights[i][j] = 1d;
        }
    }
    
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

    var surface = new NURBSSurface(ptGrid, uKnot, vKnot, Config.DegreeU, Config.DegreeV, weights);
    
    var us = new double[Config.UNums + 1];
    var points = new Point[us.Length];

    for (int i = 0; i < us.Length; i++)
    {
        us[i] = (1d / Config.UNums) * i;
    }

    for (int i = 0; i < us.Length; i++)
    {
        var r = surface.ParameterAt(us[i], us[i]);
        points[i] = new Point(r[0], r[1]);
    }
    
    var femSolution = tests.GetFuncValues(points);

    var femPath = "../../../../CMM1.View/" + Config.FolderName + "/dataFEM.txt";
    var splinePath = "../../../../CMM1.View/" + Config.FolderName + "/dataSpline.txt";
    var truePath = "../../../../CMM1.View/" + Config.FolderName + "/dataTrue.txt";
    
    Directory.CreateDirectory("../../../../CMM1.View/" + Config.FolderName);
    
    using var writerFEM = new StreamWriter(femPath);
    using var writerSpline = new StreamWriter(splinePath);
    using var writerTrue = new StreamWriter(truePath);
    using var configWriter = new StreamWriter("../../../../CMM1.View/config.txt");
    configWriter.Write(Config.FolderName);

    Console.WriteLine("FEM solution");
    
    for (var i = 0; i < points.Length; i++)
    {
        var point = points[i];
        Console.WriteLine($"{point.X:F8} {point.Y:F8} {femSolution[i].Value:E8}");
        writerFEM.WriteLine($"{point.X:F8} {point.Y:F8} {femSolution[i].Value:E8}");
    }

    Console.WriteLine("Spline solution");
    
    for (var i = 0; i < us.Length; i++)
    {
        var r = surface.ParameterAt(us[i], us[i]);
        Console.WriteLine($"{r[0]:F8} {r[1]:F8} {r[2]:E8}");
        writerSpline.WriteLine($"{r[0]:F8} {r[1]:F8} {r[2]:E8}");
    }

    Console.WriteLine("True solution");
    Func<Node2D, double, double> u = Config.u;

    foreach (var point in points)
    {
        Console.WriteLine($"{point.X:F8} {point.Y:F8} {u(new Node2D(point.X, point.Y), 1):E8}");
        writerTrue.WriteLine($"{point.X:F8} {point.Y:F8} {u(new Node2D(point.X, point.Y), 1):E8}");
    }
}

RunTest();