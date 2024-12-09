using CourseProjectDima.Core.GridComponents;

namespace CourseProjectDima.Calculus;

public class DerivativeCalculator
{
    private const double Delta = 1.0e-3;
    private Node2D _bufferPoint;

    public double Calculate(Func<Node2D, double, double> function, Node2D point, double time, char variableChar)
    {
        double result;

        if (variableChar == 'r')
        {
            _bufferPoint.Z = point.Z;

            _bufferPoint.R = point.R + Delta;
            var leftValue = function(_bufferPoint, time);

            _bufferPoint.R = point.R - Delta;
            var rightValue = function(_bufferPoint, time);

            result = leftValue - rightValue;
        }
        else if (variableChar == 'z')
        {
            _bufferPoint.R = point.R;

            _bufferPoint.Z = point.Z + Delta;
            var leftValue = function(_bufferPoint, time);

            _bufferPoint.Z = point.Z - Delta;
            var rightValue = function(_bufferPoint, time);

            result = leftValue - rightValue;
        }
        else
        {
            throw new Exception("Invalid variable");
        }

        return result / (2.0 * Delta);
    }
}