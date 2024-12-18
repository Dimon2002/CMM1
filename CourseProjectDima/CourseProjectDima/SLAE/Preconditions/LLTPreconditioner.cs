﻿using CourseProjectDima.Core.Global;

namespace CourseProjectDima.SLAE.Preconditions;

public class LLTPreconditioner : IPreconditioner<SymmetricSparseMatrix>
{
    public SymmetricSparseMatrix Decompose(SymmetricSparseMatrix globalMatrix)
    {
        var preconditionMatrix = globalMatrix;

        for (var i = 0; i < preconditionMatrix.Count; i++)
        {
            var sumD = 0.0;
            for (var j = preconditionMatrix.RowsIndexes[i]; j < preconditionMatrix.RowsIndexes[i + 1]; j++)
            {
                var sum = 0d;

                for (var k = preconditionMatrix.RowsIndexes[i]; k < j; k++)
                {
                    var iPrev = i - preconditionMatrix.ColumnsIndexes[j];
                    var kPrev = preconditionMatrix.IndexOf(i - iPrev, preconditionMatrix.ColumnsIndexes[k]);

                    if (kPrev == -1) continue;

                    var kColumn = preconditionMatrix.ColumnsIndexes[k];
                    var kPrevColumn = preconditionMatrix.ColumnsIndexes[kPrev];

                    sum += preconditionMatrix[i, kColumn] * preconditionMatrix[kPrevColumn, i - iPrev];
                }

                var jColumn = preconditionMatrix.ColumnsIndexes[j];

                preconditionMatrix[i, jColumn] =
                    (preconditionMatrix[i, jColumn] - sum) / preconditionMatrix[jColumn, jColumn];

                sumD += Math.Pow(preconditionMatrix[i, jColumn], 2);
            }

            preconditionMatrix[i, i] = Math.Sqrt(preconditionMatrix[i, i] - sumD);
        }

        return preconditionMatrix;
    }
}