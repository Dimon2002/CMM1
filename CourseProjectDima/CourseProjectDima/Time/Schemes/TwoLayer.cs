using CourseProjectDima.Core.Base;
using CourseProjectDima.Core.Global;

namespace CourseProjectDima.Time.Schemes;

public class TwoLayer
{
    private readonly SymmetricSparseMatrix _stiffnessMatrix;
    private readonly SymmetricSparseMatrix _sigmaMassMatrix;
    private SymmetricSparseMatrix _bufferMatrix;
    private Vector _bufferVector;

    public TwoLayer(SymmetricSparseMatrix stiffnessMatrix, SymmetricSparseMatrix sigmaMassMatrix)
    {
        _stiffnessMatrix = stiffnessMatrix;
        _sigmaMassMatrix = sigmaMassMatrix;
    }

    public void SetBuffers(SymmetricSparseMatrix bufferMatrix, Vector bufferVector)
    {
        _bufferMatrix = bufferMatrix;
        _bufferVector = bufferVector;
    }

    public Equation<SymmetricSparseMatrix> BuildEquation
    (
        Vector rightPart,
        Vector previousSolution,
        double currentTime,
        double previousTime
    )
    {
        var delta0 = currentTime - previousTime;

        var b = Vector.Sum
            (
                rightPart,
                SymmetricSparseMatrix.Multiply
                (
                    SymmetricSparseMatrix.Subtract
                    (
                        SymmetricSparseMatrix.Multiply
                        (
                            1 / delta0,
                            _sigmaMassMatrix,
                            _bufferMatrix
                        ),
                        _stiffnessMatrix,
                        _bufferMatrix
                    ),
                    previousSolution,
                    _bufferVector
                ),
                rightPart
            );

        var q = new Vector(rightPart.Count);

        var matrixA = SymmetricSparseMatrix.Multiply
            (
                1 / delta0,
                _sigmaMassMatrix,
                _bufferMatrix
            );

        return new Equation<SymmetricSparseMatrix>(matrixA, q, b);
    }
}