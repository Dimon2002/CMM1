using CourseProjectDima.Core.Base;
using CourseProjectDima.Core.Local;

namespace CourseProjectDima.FEM.Assembling;

public interface IInserter<in TMatrix>
{
    public void InsertMatrix(TMatrix globalMatrix, LocalMatrix localMatrix);
    public void InsertVector(Vector vector, LocalVector localVector);
}