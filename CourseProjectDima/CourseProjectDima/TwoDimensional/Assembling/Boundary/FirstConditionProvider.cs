using CourseProjectDima.Core;
using CourseProjectDima.Core.Base;
using CourseProjectDima.Core.Boundary;
using CourseProjectDima.Core.GridComponents;
using CourseProjectDima.Core.Local;

namespace CourseProjectDima.TwoDimensional.Assembling.Boundary;

public class FirstConditionProvider
{
    private readonly Grid<Node2D> _grid;
    private readonly Func<Node2D, double, double> _u;
    private FirstConditionValue[]? _conditionsValues;
    private int[][]? _indexes;
    private LocalVector[]? _values;

    public FirstConditionProvider(Grid<Node2D> grid, Func<Node2D, double, double> u)
    {
        _grid = grid;
        _u = u;
    }

    public FirstConditionValue[] GetConditions(FirstCondition[] conditions, double time)
    {
        ValidateMemory(conditions);

        for (var i = 0; i < conditions.Length; i++)
        {
            var condition = conditions[i];
            _grid.Elements[condition.ElementIndex].GetBoundNodeIndexes(condition.Bound, _indexes[i]);

            for (var j = 0; j < _indexes[i].Length; j++)
            {
                _values[i][j] = Calculate(_indexes[i][j], time);
            }

            _conditionsValues[i].Values = _values[i];
        }

        return _conditionsValues;
    }

    public FirstCondition[] GetArrays(int elementsByLength, int elementsByHeight)
    {
        var conditions = new FirstCondition[2 * (elementsByLength + elementsByHeight)];

        var j = 0;

        for (var i = 0; i < elementsByLength; i++, j++)
        {
            conditions[j] = new FirstCondition(i, Bound.Lower);
        }

        for (var i = 0; i < elementsByHeight; i++, j++)
        {
            conditions[j] = new FirstCondition(i * elementsByLength, Bound.Left);
        }

        for (var i = 0; i < elementsByHeight; i++, j++)
        {
            conditions[j] = new FirstCondition((i + 1) * elementsByLength - 1, Bound.Right);
        }

        for (var i = elementsByLength * (elementsByHeight - 1); i < elementsByLength * elementsByHeight; i++, j++)
        {
            conditions[j] = new FirstCondition(i, Bound.Upper);
        }

        return conditions;
    }

    private void ValidateMemory(FirstCondition[] conditions)
    {
        if (_conditionsValues is null || _conditionsValues.Length != conditions.Length)
        {
            _conditionsValues = new FirstConditionValue[conditions.Length];
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

    private double Calculate(int index, double time)
    {
        return _u(_grid.Nodes[index], time);
    }
}