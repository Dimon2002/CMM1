using CourseProjectDima.Core.Base;
using CourseProjectDima.Core.Global;

namespace CourseProjectDima.Time.Schemes;

public class ThreeLayer
{
    private readonly SymmetricSparseMatrix _stiffnessMatrix;
    private readonly SymmetricSparseMatrix _sigmaMassMatrix;
    private SymmetricSparseMatrix _bufferMatrix;
    private Vector _bufferVector;

    public ThreeLayer(SymmetricSparseMatrix stiffnessMatrix, SymmetricSparseMatrix sigmaMassMatrix)
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
        Vector twoLayersBackSolution,
        double currentTime,
        double previousTime,
        double twoLayersBackTime
    )
    {
        var delta01 = currentTime - previousTime;
        var delta02 = currentTime - twoLayersBackTime;
        var delta12 = previousTime - twoLayersBackTime;

        var b = Vector.Sum
            (
                Vector.Sum
                (
                    rightPart,
                    SymmetricSparseMatrix.Multiply
                    (
                        SymmetricSparseMatrix.Subtract
                        (
                            SymmetricSparseMatrix.Multiply
                            (
                                (-delta01 + delta12) / (delta01 * delta12),
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
                ),
                Vector.Multiply
                (
                    delta01 / (delta02 * delta12),
                    SymmetricSparseMatrix.Multiply
                    (
                        _sigmaMassMatrix,
                        twoLayersBackSolution,
                        _bufferVector
                    ),
                    _bufferVector
                ),
            rightPart
            );

        var q = new Vector(rightPart.Count);

        var matrixA = SymmetricSparseMatrix.Multiply
            (
                delta12 / (delta01 * delta02),
                _sigmaMassMatrix,
                _bufferMatrix
            );

        return new Equation<SymmetricSparseMatrix>(matrixA, q, b);
    }
}