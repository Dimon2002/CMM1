using CourseProjectDima.Calculus;
using CourseProjectDima.Core;
using CourseProjectDima.Core.Base;
using CourseProjectDima.Core.Boundary;
using CourseProjectDima.Core.GridComponents;
using CourseProjectDima.Core.Local;
using CourseProjectDima.TwoDimensional.Assembling.MatrixTemplates;
using CourseProjectDima.TwoDimensional.Parameters;
using Vector = CourseProjectDima.Core.Base.Vector;

namespace CourseProjectDima.TwoDimensional.Assembling.Boundary;

public class SecondConditionProvider
{
    private readonly Grid<Node2D> _grid;
    private readonly MaterialRepository _materialRepository;
    private readonly Func<Node2D, double, double> _u;
    private readonly DerivativeCalculator _derivativeCalculator;
    private readonly Matrix _templateMatrix;
    private readonly Matrix _templateRMatrix;
    private SecondConditionValue[]? _conditionsValues;
    private int[][]? _indexes;
    private LocalVector[]? _values;
    private readonly Vector _thetas = new(2);
    private readonly Vector _buffer = new(2);

    public SecondConditionProvider
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

    public SecondConditionValue[] GetConditions(SecondCondition[] conditions, double time)
    {
        ValidateMemory(conditions);

        for (var i = 0; i < conditions.Length; i++)
        {
            var condition = conditions[i];
            var element = _grid.Elements[condition.ElementIndex];
            var (indexes, h) = element.GetBoundNodeIndexes(condition.Bound, _indexes[i]);

            var lambda = _materialRepository.GetById(element.MaterialId).Lambda;

            var variable = condition.Bound is Bound.Left or Bound.Right ? 'r' : 'z';

            var thetas = GetThetas(indexes, condition.Bound, variable, lambda, time);

            if (variable == 'r')
            {
                GetRVector(thetas, h, indexes, _values[i].Vector);
            }
            else
            {
                GetZVector(thetas, h, indexes, _values[i].Vector);
            }

            _conditionsValues[i].Vector = _values[i];
        }

        return _conditionsValues;
    }

    private void ValidateMemory(SecondCondition[] conditions)
    {
        if (_conditionsValues is null || _conditionsValues.Length != conditions.Length)
        {
            _conditionsValues = new SecondConditionValue[conditions.Length];
        }

        if (_indexes is null || _indexes.GetLength(0) != _conditionsValues.Length)
        {
            _indexes = new int[_conditionsValues.Length][];

            for (var i = 0; i < _conditionsValues.Length; i++)
            {
                _indexes[i] = new int[2];
            }
        }

        if (_values is null || _values.Length != _conditionsValues.Length)
        {
            _values = new LocalVector[_conditionsValues.Length];

            for (var i = 0; i < _conditionsValues.Length; i++)
            {
                _values[i] = new LocalVector(_indexes[i], new Vector(2));
            }
        }
    }

    private Vector GetThetas(int[] indexes, Bound bound, char variable, double lambda, double time)
    {
        for (var i = 0; i < indexes.Length; i++)
        {
            _thetas[i] = _derivativeCalculator.Calculate(_u, _grid.Nodes[indexes[i]], time, variable);
        }

        if (bound is Bound.Left or Bound.Lower)
        {
            Vector.Multiply(-lambda, _thetas);
        }
        else
        {
            Vector.Multiply(lambda, _thetas);
        }

        return _thetas;
    }

    private Vector GetRVector(Vector thetas, double h, int[] indexes, Vector result)
    {
        Vector.Multiply
        (
            h * _grid.Nodes[indexes[0]].R / 6d,
            Matrix.Multiply(_templateMatrix, thetas, result),
            result
        );

        return result;
    }

    private Vector GetZVector(Vector thetas, double h, int[] indexes, Vector result)
    {
        Vector.Sum
        (
            Vector.Multiply
            (
                h * _grid.Nodes[indexes[0]].R / 6d,
                Matrix.Multiply(_templateMatrix, thetas, result),
                result
            ),
            Vector.Multiply
            (
                Math.Pow(h, 2) / 12d,
                Matrix.Multiply(_templateRMatrix, thetas, _buffer),
                _buffer
            ),
            result
        );

        return result;
    }
}