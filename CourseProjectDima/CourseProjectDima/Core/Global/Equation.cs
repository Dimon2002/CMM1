using CourseProjectDima.Core.Base;

namespace CourseProjectDima.Core.Global;

public record Equation<TMatrix>(TMatrix Matrix, Vector Solution, Vector RightPart);