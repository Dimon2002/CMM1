using CourseProjectDima.Core.Base;
using CourseProjectDima.Core.Global;
using CourseProjectDima.Core.Local;

namespace CourseProjectDima.Time.Schemes;

public class FourLayer
{
    private readonly SymmetricSparseMatrix _stiffnessMatrix;
    private readonly SymmetricSparseMatrix _massMatrix;
    private SymmetricSparseMatrix _bufferMatrix;
    private Vector _bufferVector;

    public FourLayer(SymmetricSparseMatrix stiffnessMatrix, SymmetricSparseMatrix massMatrix)
    {
        _stiffnessMatrix = stiffnessMatrix;
        _massMatrix = massMatrix;
    }

    public void SetBuffers(SymmetricSparseMatrix bufferMatrix, Vector bufferVector1)
    {
        _bufferMatrix = bufferMatrix;
        _bufferVector = bufferVector1;
    }

    public Equation<SymmetricSparseMatrix> BuildEquation
    (
        Vector rightPart,
        Vector previousSolution,
        Vector twoLayersBackSolution,
        Vector threeLayersBackSolution,
        double currentTime,
        double previousTime,
        double twoLayersBackTime,
        double threeLayersBackTime
    )
    {
        var delta01 = currentTime - previousTime;
        var delta02 = currentTime - twoLayersBackTime;
        var delta03 = currentTime - threeLayersBackTime;
        var delta12 = previousTime - twoLayersBackTime;
        var delta13 = previousTime - threeLayersBackTime;
        var delta23 = twoLayersBackTime - threeLayersBackTime;

        var b = Vector.Sum
            (
                Vector.Sum
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
                                    (delta13 * (delta01 - delta12) + delta01 * delta12) /
                                    -(delta01 * delta12 * delta13),
                                    _massMatrix,
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
                        delta01 * delta13 / (delta02 * delta12 * delta23),
                        SymmetricSparseMatrix.Multiply
                        (
                            _massMatrix,
                            twoLayersBackSolution,
                            _bufferVector
                        ),
                        _bufferVector
                    ),
                    rightPart
                ),
                Vector.Multiply
                (
                    delta01 * delta12 / -(delta03 * delta13 * delta23),
                    SymmetricSparseMatrix.Multiply
                    (
                        _massMatrix,
                        threeLayersBackSolution,
                        _bufferVector
                    ),
                    _bufferVector
                ),
                rightPart
            );

        var q = new Vector(rightPart.Count);

        var matrixA = SymmetricSparseMatrix.Multiply
        (
            delta12 * delta13 / (delta01 * delta02 * delta03),
            _massMatrix,
            _bufferMatrix
        );

        return new Equation<SymmetricSparseMatrix>(matrixA, q, b);
    }
}