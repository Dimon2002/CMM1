using CourseProjectDima.Core.GridComponents;

namespace CourseProjectDima.FEM.Parameters;

public interface IFunctionalParameter
{
    public double Calculate(int nodeIndex, double time);

    public double Calculate(Node2D node, double time);
}