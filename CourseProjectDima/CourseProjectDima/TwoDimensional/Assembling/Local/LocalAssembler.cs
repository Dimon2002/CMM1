using CourseProjectDima.Calculus;
using CourseProjectDima.Core;
using CourseProjectDima.Core.Base;
using CourseProjectDima.Core.GridComponents;
using CourseProjectDima.Core.Local;
using CourseProjectDima.FEM.Assembling.Local;
using CourseProjectDima.FEM.Parameters;
using CourseProjectDima.GridGenerator.Area.Core;
using CourseProjectDima.TwoDimensional.Parameters;
using System;

namespace CourseProjectDima.TwoDimensional.Assembling.Local;

public class LocalAssembler : ILocalAssembler
{
    private readonly Grid<Node2D> _grid;
    private readonly ILocalMatrixAssembler _localMatrixAssembler;
    private readonly MaterialRepository _materialRepository;
    private readonly IFunctionalParameter _functionalParameter;
    private readonly LocalBasisFunctionsProvider _localBasisFunctionsProvider;
    private readonly DoubleIntegralCalculator _doubleIntegralCalculator = new ();
    private readonly Vector _rightPart = new(4);
    private readonly Vector _buffer = new(4);

    public LocalAssembler
    (
        Grid<Node2D> grid,
        ILocalMatrixAssembler localMatrixAssembler,
        MaterialRepository materialRepository,
        IFunctionalParameter functionalParameter,
        LocalBasisFunctionsProvider localBasisFunctionsProvider
    )
    {
        _grid = grid;
        _localMatrixAssembler = localMatrixAssembler;
        _materialRepository = materialRepository;
        _functionalParameter = functionalParameter;
        _localBasisFunctionsProvider = localBasisFunctionsProvider;
    }

    public LocalMatrix AssembleStiffnessMatrix(Element element)
    {
        var matrix = GetStiffnessMatrix(element);

        return new LocalMatrix(element.NodesIndexes, matrix);
    }

    public LocalMatrix AssembleMassMatrix(Element element)
    {
        var matrix = GetMassMatrix(element);

        return new LocalMatrix(element.NodesIndexes, matrix);
    }

    public LocalVector AssembleRightPart(Element element, double time)
    {
        var vector = GetRightPart(element, time);

        return new LocalVector(element.NodesIndexes, vector);
    }

    private Matrix GetStiffnessMatrix(Element element)
    {
        var lambda = _materialRepository.GetById(element.MaterialId).Lambda;

        var stiffness = _localMatrixAssembler.AssembleStiffnessMatrix(element);

        return Matrix.Multiply(lambda, stiffness, stiffness);
    }

    private Matrix GetMassMatrix(Element element)
    {
        var sigma = _materialRepository.GetById(element.MaterialId).Sigma;

        var mass = _localMatrixAssembler.AssembleMassMatrix(element);

        return Matrix.Multiply(sigma, mass, mass);
    }

    private Vector GetRightPart(Element element, double time)
    {
        var mass = _localMatrixAssembler.AssembleMassMatrix(element);

        for (var i = 0; i < _rightPart.Count; i++)
        {
            _buffer[i] = _functionalParameter.Calculate(_grid.Nodes[element.NodesIndexes[i]], time);
        }

        return Matrix.Multiply(mass, _buffer, _rightPart);

        var rInterval = new Interval(_grid.Nodes[element.NodesIndexes[0]].R, _grid.Nodes[element.NodesIndexes[1]].R);
        var zInterval = new Interval(_grid.Nodes[element.NodesIndexes[0]].Z, _grid.Nodes[element.NodesIndexes[2]].Z);

        var localBasisFunctions = _localBasisFunctionsProvider.GetBilinearFunctions(element);

        for (var i = 0; i < element.NodesIndexes.Length; i++)
        {
            _rightPart[i] = _doubleIntegralCalculator.Calculate
            (
                rInterval,
                zInterval,
                (r, z) =>
                {
                    var node = new Node2D(r, z);
                    return
                        _functionalParameter.Calculate(node, time) * localBasisFunctions[i].Calculate(node) * r;
                }
            );
        }

        return _rightPart;
    }
}