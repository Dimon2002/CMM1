using CourseProjectDima.Core.Base;
using CourseProjectDima.Core.GridComponents;

namespace CourseProjectDima.FEM.Assembling.Local;

public interface ILocalMatrixAssembler
{
    public Matrix AssembleStiffnessMatrix(Element element);
    public Matrix AssembleMassMatrix(Element element);
}