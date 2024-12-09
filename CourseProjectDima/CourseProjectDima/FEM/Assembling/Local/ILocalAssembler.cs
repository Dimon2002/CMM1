using CourseProjectDima.Core.GridComponents;
using CourseProjectDima.Core.Local;

namespace CourseProjectDima.FEM.Assembling.Local;

public interface ILocalAssembler
{
    public LocalMatrix AssembleStiffnessMatrix(Element element);
    public LocalMatrix AssembleMassMatrix(Element element);
    public LocalVector AssembleRightPart(Element element, double timeLayer);
}