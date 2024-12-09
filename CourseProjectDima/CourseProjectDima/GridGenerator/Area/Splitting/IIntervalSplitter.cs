using CourseProjectDima.GridGenerator.Area.Core;

namespace CourseProjectDima.GridGenerator.Area.Splitting;

public interface IIntervalSplitter
{
    public IEnumerable<double> EnumerateValues(Interval interval);
    public int Steps { get; }
}