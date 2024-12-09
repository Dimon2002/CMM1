using CourseProjectDima.Core.Boundary;
using CourseProjectDima.Core.Global;

namespace CourseProjectDima.FEM.Assembling.Global;

public interface IGaussExcluder<TMatrix>
{
    public void Exclude(Equation<TMatrix> equation, FirstConditionValue conditionValue);
}