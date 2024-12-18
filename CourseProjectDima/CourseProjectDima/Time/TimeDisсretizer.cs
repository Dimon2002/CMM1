﻿using CourseProjectDima.Core;
using CourseProjectDima.Core.Base;
using CourseProjectDima.Core.Boundary;
using CourseProjectDima.Core.Global;
using CourseProjectDima.Core.GridComponents;
using CourseProjectDima.SLAE.Solvers;
using CourseProjectDima.Time.Schemes;
using CourseProjectDima.TwoDimensional.Assembling;
using CourseProjectDima.TwoDimensional.Assembling.Boundary;
using CourseProjectDima.TwoDimensional.Assembling.Global;

namespace CourseProjectDima.Time;

public class TimeDisсretizer
{
    private Vector PreviousSolution => _timeSolutions[_currentTimeLayer - 1];
    private Vector TwoLayersBackSolution => _timeSolutions[_currentTimeLayer - 2];
    public Vector ThreeLayersBackSolution => _timeSolutions[_currentTimeLayer - 3];

    private Vector[] _timeSolutions;

    private double CurrentTime => _timeLayers[_currentTimeLayer];
    private double PreviousTime => _timeLayers[_currentTimeLayer - 1];
    private double TwoLayersBackTime => _timeLayers[_currentTimeLayer - 2];
    private double ThreeLayersBackTime => _timeLayers[_currentTimeLayer - 3];

    private int _currentTimeLayer = 0;

    private readonly GlobalAssembler<Node2D> _globalAssembler;
    private readonly FirstConditionProvider _firstBoundaryProvider;
    private readonly GaussExcluder _gaussExcluder;
    private readonly SecondConditionProvider _secondBoundaryProvider;
    private readonly ThirdConditionProvider _thirdBoundaryProvider;
    private readonly Inserter _inserter;

    private Grid<Node2D> _grid;
    private double[] _timeLayers;
    private TwoLayer? _twoLayer;
    private ThreeLayer? _threeLayer;
    private FourLayer? _fourLayer;
    private FirstCondition[]? _firstConditions;
    private SecondCondition[]? _secondConditions;
    private ThirdCondition[]? _thirdConditions;
    private MCG _solver;

    public TimeDisсretizer
    (
        GlobalAssembler<Node2D> globalAssembler,
        FirstConditionProvider firstBoundaryProvider,
        GaussExcluder gaussExcluder,
        SecondConditionProvider secondBoundaryProvider,
        ThirdConditionProvider thirdBoundaryProvider,
        Inserter inserter
    )
    {
        _globalAssembler = globalAssembler;
        _firstBoundaryProvider = firstBoundaryProvider;
        _gaussExcluder = gaussExcluder;
        _secondBoundaryProvider = secondBoundaryProvider;
        _thirdBoundaryProvider = thirdBoundaryProvider;
        _inserter = inserter;
    }

    public TimeDisсretizer SetGrid(Grid<Node2D> grid)
    {
        _grid = grid;

        return this;
    }

    public TimeDisсretizer SetTimeLayers(double[] timeLayers)
    {
        _timeLayers = timeLayers;
        _timeSolutions = new Vector[_timeLayers.Length];

        return this;
    }

    public TimeDisсretizer SetInitialSolution(Func<Node2D, double, double> u)
    {
        var initialSolution = new Vector(_grid.Nodes.Length);
        var currentTime = CurrentTime;

        for (var i = 0; i < _grid.Nodes.Length; i++)
        {
            initialSolution[i] = u(_grid.Nodes[i], currentTime);
        }

        _timeSolutions[_currentTimeLayer] = initialSolution;
        _currentTimeLayer++;

        return this;
    }

    public TimeDisсretizer SetFirstConditions(FirstCondition[] conditions)
    {
        _firstConditions = conditions;

        return this;
    }

    public TimeDisсretizer SetSecondConditions(SecondCondition[] conditions)
    {
        _secondConditions = conditions;

        return this;
    }

    public TimeDisсretizer SetThirdConditions(ThirdCondition[] conditions)
    {
        _thirdConditions = conditions;

        return this;
    }

    public TimeDisсretizer SetSolver(MCG solver)
    {
        _solver = solver;

        return this;
    }

    public Vector[] GetSolutions()
    {
        var stiffness = _globalAssembler.AssembleStiffnessMatrix(_grid);
        var mass = _globalAssembler.AssembleMassMatrix(_grid);
        var preconditionMatrix = stiffness.Clone();
        _solver.SetPrecondition(preconditionMatrix);

        var bufferVector = new Vector(stiffness.Count);
        var bufferMatrix = stiffness.Clone();

        var boundaryAppliсator = AssemblyBoundaryApplicator();

        Equation<SymmetricSparseMatrix> equation;

        if (_timeSolutions[1] == null)
        {
            _twoLayer = new TwoLayer(stiffness, mass);
            _twoLayer.SetBuffers(bufferMatrix, bufferVector);
            equation = UseTwoLayerScheme();

            boundaryAppliсator(equation);

            _timeSolutions[_currentTimeLayer] = _solver.Solve(equation);
            _currentTimeLayer++;
        }

        if (_timeSolutions[2] == null)
        {
            _threeLayer = new ThreeLayer(stiffness, mass);
            _threeLayer.SetBuffers(bufferMatrix, bufferVector);
            equation = UseThreeLayerScheme();

            boundaryAppliсator(equation);

            _timeSolutions[_currentTimeLayer] = _solver.Solve(equation);
            _currentTimeLayer++;
        }

        _fourLayer = new FourLayer(stiffness, mass);
        _fourLayer.SetBuffers(bufferMatrix, bufferVector);

        while (_currentTimeLayer < _timeLayers.Length)
        {
            equation = UseFourLayerScheme();

            boundaryAppliсator(equation);

            _timeSolutions[_currentTimeLayer] = _solver.Solve(equation);
            _currentTimeLayer++;
        }

        return _timeSolutions;
    }

    private Equation<SymmetricSparseMatrix> UseTwoLayerScheme()
    {
        var equation = _twoLayer
            .BuildEquation
            (
                _globalAssembler.AssembleRightPart(_grid, PreviousTime),
                PreviousSolution,
                CurrentTime,
                PreviousTime
            );

        return equation;
    }

    private Equation<SymmetricSparseMatrix> UseThreeLayerScheme()
    {
        var equation = _threeLayer
            .BuildEquation
            (
                _globalAssembler.AssembleRightPart(_grid, PreviousTime),
                PreviousSolution,
                TwoLayersBackSolution,
                CurrentTime,
                PreviousTime,
                TwoLayersBackTime
            );

        return equation;
    }

    private Equation<SymmetricSparseMatrix> UseFourLayerScheme()
    {
        var equation = _fourLayer
            .BuildEquation
            (
                _globalAssembler.AssembleRightPart(_grid, PreviousTime),
                PreviousSolution,
                TwoLayersBackSolution,
                ThreeLayersBackSolution,
                CurrentTime,
                PreviousTime,
                TwoLayersBackTime,
                ThreeLayersBackTime
            );

        return equation;
    }

    public delegate void BoundaryAppliсator(Equation<SymmetricSparseMatrix> equation);

    public BoundaryAppliсator AssemblyBoundaryApplicator()
    {
        BoundaryAppliсator boundaryAppliсator = null;

        if (_secondConditions != null)
        {
            boundaryAppliсator += delegate (Equation<SymmetricSparseMatrix> equation)
            {
                var secondConditions =
                    _secondBoundaryProvider.GetConditions(_secondConditions, CurrentTime);
                ApplySecondConditions(equation, secondConditions);
            };
        }

        if (_thirdConditions != null)
        {
            boundaryAppliсator += delegate (Equation<SymmetricSparseMatrix> equation)
            {
                var thirdConditions =
                    _thirdBoundaryProvider.GetConditions(_thirdConditions, CurrentTime);
                ApplyThirdConditions(equation, thirdConditions);
            };
        }

        if (_firstConditions != null)
        {
            boundaryAppliсator += delegate (Equation<SymmetricSparseMatrix> equation)
            {
                var firstConditions =
                    _firstBoundaryProvider.GetConditions(_firstConditions, CurrentTime);
                ApplyFirstConditions(equation, firstConditions);
            };
        }

        return boundaryAppliсator;
    }

    private void ApplyFirstConditions(Equation<SymmetricSparseMatrix> equation, FirstConditionValue[] firstConditions)
    {
        foreach (var firstCondition in firstConditions)
        {
            _gaussExcluder.Exclude(equation, firstCondition);
        }
    }

    private void ApplySecondConditions(Equation<SymmetricSparseMatrix> equation, SecondConditionValue[] secondConditions)
    {
        foreach (var secondCondition in secondConditions)
        {
            _inserter.InsertVector(equation.RightPart, secondCondition.Vector);
        }
    }

    private void ApplyThirdConditions(Equation<SymmetricSparseMatrix> equation, ThirdConditionValue[] thirdConditions)
    {
        foreach (var thirdCondition in thirdConditions)
        {
            _inserter.InsertMatrix(equation.Matrix, thirdCondition.Matrix);
            _inserter.InsertVector(equation.RightPart, thirdCondition.Vector);
        }
    }
}