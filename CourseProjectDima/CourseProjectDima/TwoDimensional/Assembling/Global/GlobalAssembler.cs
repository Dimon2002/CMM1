using CourseProjectDima.Core;
using CourseProjectDima.Core.Base;
using CourseProjectDima.Core.Global;
using CourseProjectDima.FEM.Assembling;
using CourseProjectDima.FEM.Assembling.Local;

namespace CourseProjectDima.TwoDimensional.Assembling.Global;

public class GlobalAssembler<TNode>
{
    private readonly IMatrixPortraitBuilder<TNode, SymmetricSparseMatrix> _matrixPortraitBuilder;
    private readonly ILocalAssembler _localAssembler;
    private readonly IInserter<SymmetricSparseMatrix> _inserter;

    public GlobalAssembler
    (
        IMatrixPortraitBuilder<TNode, SymmetricSparseMatrix> matrixPortraitBuilder,
        ILocalAssembler localAssembler,
        IInserter<SymmetricSparseMatrix> inserter
    )
    {
        _matrixPortraitBuilder = matrixPortraitBuilder;
        _localAssembler = localAssembler;
        _inserter = inserter;
    }

    public SymmetricSparseMatrix AssembleStiffnessMatrix(Grid<TNode> grid)
    {
        var globalMatrix = _matrixPortraitBuilder.Build(grid);

        foreach (var element in grid)
        {
            var localMatrix = _localAssembler.AssembleStiffnessMatrix(element);

            _inserter.InsertMatrix(globalMatrix, localMatrix);
        }

        return globalMatrix;
    }

    public SymmetricSparseMatrix AssembleMassMatrix(Grid<TNode> grid)
    {
        var globalMatrix = _matrixPortraitBuilder.Build(grid);

        foreach (var element in grid)
        {
            var localMatrix = _localAssembler.AssembleMassMatrix(element);

            _inserter.InsertMatrix(globalMatrix, localMatrix);
        }

        return globalMatrix;
    }

    public Vector AssembleRightPart(Grid<TNode> grid, double time)
    {
        var rightPart = new Vector(grid.Nodes.Length);

        foreach (var element in grid)
        {
            var localRightPart = _localAssembler.AssembleRightPart(element, time);

            _inserter.InsertVector(rightPart, localRightPart);
        }

        return rightPart;
    }
}