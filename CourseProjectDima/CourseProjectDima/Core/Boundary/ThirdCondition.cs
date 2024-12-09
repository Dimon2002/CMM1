using CourseProjectDima.Core.GridComponents;
using CourseProjectDima.Core.Local;

namespace CourseProjectDima.Core.Boundary;

public record struct ThirdCondition(int ElementIndex, Bound Bound, double Beta);
public record struct ThirdConditionValue(LocalMatrix Matrix, LocalVector Vector);