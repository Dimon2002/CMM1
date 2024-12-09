using CourseProjectDima.Core.Base;

namespace CourseProjectDima.TwoDimensional.Assembling.MatrixTemplates;

public class StiffnessMatrixTemplatesProvider
{
    public static Matrix StiffnessMatrix => new(
        new[,]
        {
            { 1d, -1d },
            { -1d, 1d }
        });
}