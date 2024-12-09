using CourseProjectDima.Core;
using CourseProjectDima.Core.Base;
using CourseProjectDima.Core.GridComponents;
using CourseProjectDima.FEM;
using CourseProjectDima.GridGenerator.Area.Core;
using CourseProjectDima.SLAE;
using CourseProjectDima.TwoDimensional.Assembling.Local;

namespace CourseProjectDima.TwoDimensional;

public class FEMSolution
{
    private readonly Grid<Node2D> _grid;
    private readonly Vector[] _solutions;
    private readonly double[] _timeLayers;
    private readonly LocalBasisFunctionsProvider _basisFunctionsProvider;
    private readonly Func<double, double>[] _lagrangePolynomials = new Func<double, double>[2];

    public FEMSolution(Grid<Node2D> grid, Vector[] solutions, double[] timeLayers, LocalBasisFunctionsProvider basisFunctionsProvider)
    {
        _grid = grid;
        _solutions = solutions;
        _timeLayers = timeLayers;
        _basisFunctionsProvider = basisFunctionsProvider;
    }

    public double Calculate(Node2D point, double time)
    {
        if (TimeLayersHas(time) && AreaHas(point))
        {
            var currentTimeLayerIndex = FindCurrentTimeLayer(time);

            var element = _grid.Elements.First(x => ElementHas(x, point));

            var basisFunctions = _basisFunctionsProvider.GetBilinearFunctions(element);

            var lagrangePolynomials = CreateLagrangePolynomials(currentTimeLayerIndex);

            var sum = 0d;
            for (var j = 0; j < lagrangePolynomials.Length; j++)
            {
                sum += element.NodesIndexes
                    .Select((t, i) => _solutions[currentTimeLayerIndex - j][t] * basisFunctions[i].Calculate(point))
                    .Sum() * lagrangePolynomials[j](time);
            }

            // Informer.WriteSolution(point, time, sum);

            return sum;
        }

        Informer.WriteAreaInfo();
        Informer.WriteSolution(point, double.NaN, double.NaN);
        return double.NaN;
    }

    public double CalcError(Func<Node2D, double, double> u, double time)
    {
        var solution = new Vector(_solutions[0].Count);
        var trueSolution = new Vector(_solutions[0].Count);

        for (var i = 0; i < _solutions[0].Count; i++)
        {
            solution[i] = Calculate(_grid.Nodes[i], time);
            trueSolution[i] = u(_grid.Nodes[i], time);
        }

        Vector.Subtract(solution, trueSolution, trueSolution);

        return trueSolution.Norm;
    }

    private bool ElementHas(Element element, Node2D node)
    {
        var lowerLeftCorner = _grid.Nodes[element.NodesIndexes[0]];
        var upperRightCorner = _grid.Nodes[element.NodesIndexes[^1]];
        return (node.R > lowerLeftCorner.R ||
                Math.Abs(node.R - lowerLeftCorner.R) < MethodsConfig.EpsDouble) &&
               (node.Z > lowerLeftCorner.Z ||
                Math.Abs(node.Z - lowerLeftCorner.Z) < MethodsConfig.EpsDouble) &&
               (node.R < upperRightCorner.R ||
                Math.Abs(node.R - upperRightCorner.R) < MethodsConfig.EpsDouble) &&
               (node.Z < upperRightCorner.Z ||
                Math.Abs(node.Z - upperRightCorner.Z) < MethodsConfig.EpsDouble);
    }

    private bool AreaHas(Node2D node)
    {
        var lowerLeftCorner = _grid.Nodes[0];
        var upperRightCorner = _grid.Nodes[^1];
        return (node.R > lowerLeftCorner.R ||
                Math.Abs(node.R - lowerLeftCorner.R) < MethodsConfig.EpsDouble) &&
               (node.Z > lowerLeftCorner.Z ||
                Math.Abs(node.Z - lowerLeftCorner.Z) < MethodsConfig.EpsDouble) &&
               (node.R < upperRightCorner.R ||
                Math.Abs(node.R - upperRightCorner.R) < MethodsConfig.EpsDouble) &&
               (node.Z < upperRightCorner.Z ||
                Math.Abs(node.Z - upperRightCorner.Z) < MethodsConfig.EpsDouble);
    }

    private bool TimeLayersHas(double time)
    {
        var interval = new Interval(_timeLayers[0], _timeLayers[^1]);
        return interval.Has(time);
    }

    private int FindCurrentTimeLayer(double time)
    {
        var index = Array.FindIndex(_timeLayers, x => time <= x);
        return index == 0 ? 1 : index;
    }

    private Func<double, double>[] CreateLagrangePolynomials(int timeLayerIndex)
    {
        switch (timeLayerIndex)
        {
            case 1:
                {
                    var currentTime = _timeLayers[timeLayerIndex];
                    var previousTime = _timeLayers[timeLayerIndex - 1];

                    _lagrangePolynomials[0] = t => (t - currentTime) / (previousTime - currentTime);
                    _lagrangePolynomials[1] = t => (t - previousTime) / (currentTime - previousTime);

                    return _lagrangePolynomials;
                }
            case 2:
                {
                    var currentTime = _timeLayers[timeLayerIndex];
                    var previousTime = _timeLayers[timeLayerIndex - 1];
                    var twoLayersBackTime = _timeLayers[timeLayerIndex - 2];

                    _lagrangePolynomials[0] = t => (t - twoLayersBackTime) * (t - previousTime) /
                                                   ((currentTime - twoLayersBackTime) * (currentTime - previousTime));
                    _lagrangePolynomials[1] = t => -(t - twoLayersBackTime) * (t - currentTime) /
                                                   ((currentTime - previousTime) * (previousTime - twoLayersBackTime));
                    _lagrangePolynomials[2] = t => (t - previousTime) * (t - currentTime) / 
                                                   ((currentTime - twoLayersBackTime) * (previousTime - twoLayersBackTime));

                    return _lagrangePolynomials;
                }
            default:
                {
                    var currentTime = _timeLayers[timeLayerIndex];
                    var previousTime = _timeLayers[timeLayerIndex - 1];
                    var twoLayersBackTime = _timeLayers[timeLayerIndex - 2];
                    var threeLayersBackTime = _timeLayers[timeLayerIndex - 3];

                    _lagrangePolynomials[0] = t =>
                        (t - threeLayersBackTime) * (t - twoLayersBackTime) * (t - previousTime) /
                        ((currentTime - threeLayersBackTime) * (currentTime - twoLayersBackTime) * (currentTime - previousTime));
                    _lagrangePolynomials[1] = t =>
                        (t - threeLayersBackTime) * (t - twoLayersBackTime) * (t - currentTime) /
                        ((previousTime - threeLayersBackTime) * (previousTime - twoLayersBackTime) * (previousTime - currentTime));
                    _lagrangePolynomials[2] = t => 
                        (t - threeLayersBackTime) * (t - previousTime) * (t - currentTime) /
                        ((twoLayersBackTime - threeLayersBackTime) * (twoLayersBackTime - previousTime) * (twoLayersBackTime - currentTime));
                    _lagrangePolynomials[3] = t => 
                        (t - twoLayersBackTime) * (t - previousTime) * (t - currentTime) /
                        ((threeLayersBackTime - twoLayersBackTime) * (threeLayersBackTime - previousTime) * (threeLayersBackTime - currentTime));

                    return _lagrangePolynomials;
                }
        }
    }
}