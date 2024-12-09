using CourseProjectDima;
using CourseProjectDima.Calculus;
using CourseProjectDima.Core.GridComponents;
using CourseProjectDima.FEM.Assembling.Local;
using CourseProjectDima.GridGenerator;
using CourseProjectDima.GridGenerator.Area.Splitting;
using CourseProjectDima.SLAE.Preconditions;
using CourseProjectDima.SLAE.Solvers;
using CourseProjectDima.Time;
using CourseProjectDima.TwoDimensional;
using CourseProjectDima.TwoDimensional.Assembling;
using CourseProjectDima.TwoDimensional.Assembling.Boundary;
using CourseProjectDima.TwoDimensional.Assembling.Global;
using CourseProjectDima.TwoDimensional.Assembling.Local;
using CourseProjectDima.TwoDimensional.Parameters;
using SharpMath.Geometry;
using SharpMath.Geometry._2D;
using SharpMath.Splines;
using Interval = CourseProjectDima.GridGenerator.Area.Core.Interval;

namespace CMM1;

public class Tests
{
    private readonly FEMSolution _solution;

    public Tests()
    {
        const int split = 10;

        var gridBuilder2D = new GridBuilder2D();
        var grid = gridBuilder2D
            .SetRAxis(new AxisSplitParameter(
                    [1, 11d],
                    new UniformSplitter(split)
                )
            )
            .SetZAxis(new AxisSplitParameter(
                    [1, 11d],
                    new UniformSplitter(split)
                )
            )
            .Build();

        var materialRepository = new MaterialRepository
        (
            new List<double> { 1d },
            new List<double> { 0d }
        );

        var localMatrixAssembler = new LocalMatrixAssembler(grid);

        var localBasisFunctionsProvider = new LocalBasisFunctionsProvider(grid, new LinearFunctionsProvider());

        Func<Node2D, double, double> u = Config.u;
        var f = new RightPartParameter(Config.f, grid);
        
        var derivativeCalculator = new DerivativeCalculator();
        var localAssembler = new LocalAssembler(grid, localMatrixAssembler, materialRepository, f, localBasisFunctionsProvider);

        var inserter = new Inserter();
        var globalAssembler = new GlobalAssembler<Node2D>(new MatrixPortraitBuilder(), localAssembler, inserter);

        var timeLayers =
            //new ProportionalSplitter(80, 1.25)
            new UniformSplitter(2)
            .EnumerateValues(new Interval(0, 2))
            .ToArray();

        var firstBoundaryProvider = new FirstConditionProvider(grid, u);
        var secondBoundaryProvider = new SecondConditionProvider(grid, materialRepository, u, derivativeCalculator);
        var thirdBoundaryProvider = new ThirdConditionProvider(grid, materialRepository, u, derivativeCalculator);

        var lltPreconditioner = new LLTPreconditioner();
        var solver = new MCG(lltPreconditioner, new LLTSparse());

        var timeDiscreditor = new TimeDisсretizer(globalAssembler, firstBoundaryProvider,
            new GaussExcluder(), secondBoundaryProvider, thirdBoundaryProvider, inserter);

        var solutions =
            timeDiscreditor
                .SetGrid(grid)
                .SetTimeLayers(timeLayers)
                .SetInitialSolution(u)
                .SetInitialSolution(u)
                //.SetSecondConditions
                //(
                //    new SecondCondition[]
                //    {
                //        new(0, Bound.Lower), new(0, Bound.Right)
                //    }
                //)
                //.SetThirdConditions
                //(
                //    new ThirdCondition[]
                //    {
                //        new(0, Bound.Lower, 1), new(0, Bound.Right, 1)
                //    }
                //)
                //.SetFirstConditions
                //(
                //    new FirstCondition[]
                //    {
                //        new(0, Bound.Left), new(0, Bound.Upper)
                //    }
                //)
                .SetFirstConditions(firstBoundaryProvider.GetArrays(split, split))
                .SetSolver(solver)
                .GetSolutions();

        _solution = new FEMSolution(grid, solutions, timeLayers, localBasisFunctionsProvider);
    }

    public FuncValue[] GetFuncValues(IPointsCollection<Point> points)
    {
        var funcValues = new FuncValue[points.TotalPoints];

        for (var i = 0; i < funcValues.Length; i++)
        {
            var point = points[i];
            funcValues[i] = new FuncValue(point, _solution.Calculate(new Node2D(point.X, point.Y), 1));
        }

        return funcValues;
    }

    public FuncValue[] GetFuncValues(Point[] points)
    {
        var funcValues = new FuncValue[points.Length];

        for (var i = 0; i < funcValues.Length; i++)
        {
            var point = points[i];
            funcValues[i] = new FuncValue(point, _solution.Calculate(new Node2D(point.X, point.Y), 1));
        }

        return funcValues;
    }

    /*public FuncValue[] GetDerivativeByXValues(IPointsCollection<Point> points)
    {
        var funcValues = new FuncValue[points.TotalPoints];

        for (var i = 0; i < funcValues.Length; i++)
        {
            var point = points[i];
            funcValues[i] = new FuncValue(point, _solution.CalculateDerivativeByX(new Node2D(point.X, point.Y), 1));
        }

        return funcValues;
    }*/

    /*public FuncValue[] GetDerivativeByYValues(IPointsCollection<Point> points)
    {
        var funcValues = new FuncValue[points.TotalPoints];

        for (var i = 0; i < funcValues.Length; i++)
        {
            var point = points[i];
            funcValues[i] = new FuncValue(point, _solution.CalculateDerivativeByY(new Node2D(point.X, point.Y), 1));
        }

        return funcValues;
    }*/

    public Point[] GetPoints(int split)
    {
        var gridBuilder2D = new GridBuilder2D();

        var grid = gridBuilder2D
            .SetRAxis(new AxisSplitParameter(
                    [1d, 11d],
                    //new QuadraticUniformSplitter(split)
                    new UniformSplitter(split)
                )
            )
            .SetZAxis(new AxisSplitParameter(
                    [1d, 11d],
                    //new QuadraticUniformSplitter(split)
                    new UniformSplitter(split)
                )
            )
            .Build();

        var points = new Point[grid.Nodes.Length];

        for (var i = 0; i < points.Length; i++)
        {
            var point = grid.Nodes[i];
            points[i] = new Point(point.R, point.Z);
        }

        return points;
    }
}