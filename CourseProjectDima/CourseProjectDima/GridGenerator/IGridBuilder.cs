using CourseProjectDima.Core;

namespace CourseProjectDima.GridGenerator;

public interface IGridBuilder<TPoint>
{
    public Grid<TPoint> Build();
}