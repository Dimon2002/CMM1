using CourseProjectDima.Core.GridComponents;
using CourseProjectDima.Core.Local;

namespace CourseProjectDima.Core.Boundary;

public record struct SecondCondition(int ElementIndex, Bound Bound);
public record struct SecondConditionValue(LocalVector Vector);