using CourseProjectDima.Core.GridComponents;
using CourseProjectDima.Core.Local;

namespace CourseProjectDima.Core.Boundary;

public record struct FirstCondition(int ElementIndex, Bound Bound);
public record struct FirstConditionValue(LocalVector Values);