using CourseProjectDima.Calculus;
using CourseProjectDima.Core;
using CourseProjectDima.Core.Base;
using CourseProjectDima.Core.Boundary;
using CourseProjectDima.Core.GridComponents;
using CourseProjectDima.Core.Local;
using CourseProjectDima.TwoDimensional.Assembling.MatrixTemplates;
using CourseProjectDima.TwoDimensional.Parameters;

namespace CourseProjectDima.TwoDimensional.Assembling.Boundary;

public class ThirdConditionProvider
{
    private readonly Grid<Node2D> _grid;
    private readonly MaterialRepository _materialRepository;
    private readonly Func<Node2D, double, double> _u;
    private readonly DerivativeCalculator _derivativeCalculator;
    private readonly Matrix _templateMatrix;
    private readonly Matrix _templateRMatrix;
    private ThirdConditionValue[]? _conditionsValues;
    private int[][]? _indexes;
    private LocalMatrix[]? _matrices;
    private LocalVector[]? _vectors;
    private Vector _us = new(2);
    private Vector _vectorBuffer = new(2);
    private Matrix _matrixBuffer = new(2);

    public ThirdConditionProvider
    (
        Grid<Node2D> grid,
        MaterialRepository materialRepository,
        Func<Node2D, double, double> u,
        DerivativeCalculator derivativeCalculator
    )
    {
        _grid = grid;
        _materialRepository = materialRepository;
        _u = u;
        _derivativeCalculator = derivativeCalculator;
        _templateMatrix = MassMatrixTemplatesProvider.MassMatrix;
        _templateRMatrix = MassMatrixTemplatesProvider.MassRMatrix;
    }

    public ThirdConditionValue[] GetConditions(ThirdCondition[] conditions, double time)
    {
        ValidateMemory(conditions);

        for (var i = 0; i < conditions.Length; i++)
        {
            var condition = conditions[i];
            var element = _grid.Elements[condition.ElementIndex];
            var (indexes, h) = element.GetBoundNodeIndexes(condition.Bound, _indexes[i]);
            var lambda = _materialRepository.GetById(element.MaterialId).Lambda;

            var variable = condition.Bound is Bound.Left or Bound.Right ? 'r' : 'z';

            var uS = GetUs(indexes, condition.Bound, lambda, condition.Beta, time, variable);

            if (variable == 'r')
            {
                GetRMatrix(h, condition.Beta, indexes, _matrices[i].Matrix);
            }
            else
            {
                GetZMatrix(h, condition.Beta, indexes, _matrices[i].Matrix);
            }

            GetVector(_matrices[i].Matrix, uS, _vectors[i].Vector);

            _conditionsValues[i].Matrix = _matrices[i];
            _conditionsValues[i].Vector = _vectors[i];
        }

        return _conditionsValues;
    }

    private void ValidateMemory(ThirdCondition[] conditions)
    {
        if (_conditionsValues is null || _conditionsValues.Length != conditions.Length)
        {
            _conditionsValues = new ThirdConditionValue[conditions.Length];
        }

        if (_indexes is null || _indexes.GetLength(0) != _conditionsValues.Length)
        {
            _indexes = new int[_conditionsValues.Length][];

            for (var i = 0; i < _conditionsValues.Length; i++)
            {
                _indexes[i] = new int[2];
            }
        }

        if (_vectors is null || _vectors.Length != _conditionsValues.Length)
        {
            _vectors = new LocalVector[_conditionsValues.Length];

            for (var i = 0; i < _conditionsValues.Length; i++)
            {
                _vectors[i] = new LocalVector(_indexes[i], new Vector(2));
            }
        }

        if (_matrices is null || _matrices.Length != _conditionsValues.Length)
        {
            _matrices = new LocalMatrix[_conditionsValues.Length];

            for (var i = 0; i < _conditionsValues.Length; i++)
            {
                _matrices[i] = new LocalMatrix(_indexes[i], new Matrix(2));
            }
        }
    }

    private Vector GetUs(int[] indexes, Bound bound, double lambda, double beta, double time, char variable)
    {
        for (var i = 0; i < indexes.Length; i++)
        {
            _us[i] = _derivativeCalculator.Calculate(_u, _grid.Nodes[indexes[i]], time, variable);
        }

        if (bound is Bound.Left or Bound.Lower)
        {
            Vector.Multiply(-lambda, _us);
        }
        else
        {
            Vector.Multiply(lambda, _us);
        }

        for (var i = 0; i < _us.Count; i++)
        {
            _us[i] = (_us[i] + beta * _u(_grid.Nodes[indexes[i]], time)) / beta;
        }

        return _us;
    }

    private Vector GetVector(Matrix matrix, Vector uS, Vector result)
    {
        Matrix.Multiply(matrix, uS, result);

        return result;
    }

    private Matrix GetRMatrix(double h, double beta, int[] indexes, Matrix result)
    {
        Matrix.Multiply(beta * h * _grid.Nodes[indexes[0]].R / 6d, _templateMatrix, result);

        return result;
    }

    private Matrix GetZMatrix(double h, double beta, int[] indexes, Matrix result)
    {
        Matrix.Sum
        (
            Matrix.Multiply
            (
                beta * h * _grid.Nodes[indexes[0]].R / 6d,
                _templateMatrix,
                result
            ),
            Matrix.Multiply
            (
                Math.Pow(h, 2) / 12d,
                _templateRMatrix,
                _matrixBuffer
            ),
            result
        );

        return result;
    }
}