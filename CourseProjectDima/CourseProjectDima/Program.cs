using CourseProjectDima.Core.GridComponents;
using CourseProjectDima.FEM.Assembling.Local;
using CourseProjectDima.GridGenerator;
using CourseProjectDima.GridGenerator.Area.Core;
using CourseProjectDima.GridGenerator.Area.Splitting;
using CourseProjectDima.SLAE.Preconditions;
using CourseProjectDima.SLAE.Solvers;
using CourseProjectDima.TwoDimensional;
using CourseProjectDima.TwoDimensional.Assembling;
using CourseProjectDima.TwoDimensional.Assembling.Boundary;
using CourseProjectDima.TwoDimensional.Assembling.Global;
using CourseProjectDima.TwoDimensional.Assembling.Local;
using CourseProjectDima.TwoDimensional.Parameters;
using System.Globalization;
using CourseProjectDima.Calculus;
using CourseProjectDima.Core.Boundary;
using CourseProjectDima.Time;

Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

var gridBuilder2D = new GridBuilder2D();
var grid = gridBuilder2D
    .SetRAxis(new AxisSplitParameter(
            [1d, 3d],
            new UniformSplitter(2)
        )
    )
    .SetZAxis(new AxisSplitParameter(
            [1d, 3d],
            new UniformSplitter(2)
        )
    )
    .Build();

var materialRepository = new MaterialRepository
(
    new List<double> { 1d },
    new List<double> { 1d }
);

var localMatrixAssembler = new LocalMatrixAssembler(grid);

var localBasisFunctionsProvider = new LocalBasisFunctionsProvider(grid, new LinearFunctionsProvider());

//Func<Node2D, double, double> u = (p, t) => Math.Pow(p.R, 1) + Math.Pow(p.Z, 1) - Math.Pow(t, 5);

//var f = new RightPartParameter((p, t) => -1 / p.R - 5 * Math.Pow(t, 4), grid);

Func<Node2D, double, double> u = (p, t) => Math.Pow(p.R, 1) + Math.Pow(p.Z, 1) - Math.Exp(t);

var f = new RightPartParameter((p, t) => -1 / p.R - Math.Exp(t), grid);

var derivativeCalculator = new DerivativeCalculator();

var localAssembler = new LocalAssembler(grid, localMatrixAssembler, materialRepository, f, localBasisFunctionsProvider);

var inserter = new Inserter();
var globalAssembler = new GlobalAssembler<Node2D>(new MatrixPortraitBuilder(), localAssembler, inserter);

var timeLayers =
    //new ProportionalSplitter(80, 1.25)
    new UniformSplitter(20)
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
        .SetInitialSolution(u)
        //.SetSecondConditions
        //(
        //    new SecondCondition[]
        //    {
        //        new(0, Bound.Upper), new(0, Bound.Right)
        //    }
        //)
        //.SetThirdConditions
        //(
        //    new ThirdCondition[]
        //    {
        //        new(0, Bound.Upper, 1), new(0, Bound.Right, 1)
        //    }
        //)
        //.SetFirstConditions
        //(
        //    new FirstCondition[]
        //    {
        //        new(0, Bound.Left), new(0, Bound.Lower)
        //    }
        //)
        .SetFirstConditions(firstBoundaryProvider.GetArrays(2, 2))
        .SetSolver(solver)
        .GetSolutions();

var femSolution = new FEMSolution(grid, solutions, timeLayers, localBasisFunctionsProvider);
var result = femSolution.Calculate(new Node2D(1.5d, 1.5d), 0.5);

var trueValue = u(new Node2D(1.5d, 1.5d), 0.5);

Console.WriteLine($"{result} {trueValue} {Math.Abs(trueValue - result)/Math.Abs(trueValue)}");