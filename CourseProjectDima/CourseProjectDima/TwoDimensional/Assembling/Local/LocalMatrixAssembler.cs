﻿using CourseProjectDima.Core;
using CourseProjectDima.Core.Base;
using CourseProjectDima.Core.GridComponents;
using CourseProjectDima.FEM.Assembling.Local;
using CourseProjectDima.TwoDimensional.Assembling.MatrixTemplates;

namespace CourseProjectDima.TwoDimensional.Assembling.Local;

public class LocalMatrixAssembler : ILocalMatrixAssembler
{
    private readonly Grid<Node2D> _grid;
    private readonly Matrix _stiffnessTemplate;
    private readonly Matrix _massTemplate;
    private readonly Matrix _massRTemplate;
    private readonly Matrix _stiffness = new(4);
    private readonly Matrix _stiffnessR = new(2);
    private readonly Matrix _stiffnessZ = new(2);
    private readonly Matrix _mass = new(4);
    private readonly Matrix _massR = new(2);
    private readonly Matrix _massZ = new(2);

    public LocalMatrixAssembler
    (
        Grid<Node2D> grid
    )
    {
        _grid = grid;
        _stiffnessTemplate = StiffnessMatrixTemplatesProvider.StiffnessMatrix;
        _massTemplate = MassMatrixTemplatesProvider.MassMatrix;
        _massRTemplate = MassMatrixTemplatesProvider.MassRMatrix;
    }

    public Matrix AssembleStiffnessMatrix(Element element)
    {
        var stiffnessR = AssembleStiffnessR(element);
        var stiffnessZ = AssembleStiffnessZ(element);

        var massR = AssembleMassR(element);
        var massZ = AssembleMassZ(element);

        for (var i = 0; i < element.NodesIndexes.Length; i++)
        {
            for (var j = 0; j <= i; j++)
            {
                _stiffness[i, j] = stiffnessR[GetMuIndex(i), GetMuIndex(j)] * massZ[GetNuIndex(i), GetNuIndex(j)] +
                                   massR[GetMuIndex(i), GetMuIndex(j)] * stiffnessZ[GetNuIndex(i), GetNuIndex(j)];
                _stiffness[j, i] = _stiffness[i, j];
            }
        }

        return _stiffness;
    }

    public Matrix AssembleMassMatrix(Element element)
    {
        var massR = AssembleMassR(element);
        var massZ = AssembleMassZ(element);

        for (var i = 0; i < element.NodesIndexes.Length; i++)
        {
            for (var j = 0; j <= i; j++)
            {
                _mass[i, j] = massR[GetMuIndex(i), GetMuIndex(j)] * massZ[GetNuIndex(i), GetNuIndex(j)];
                _mass[j, i] = _mass[i, j];
            }
        }

        return _mass;
    }

    private Matrix AssembleStiffnessR(Element element)
    {
        Matrix.Multiply((_grid.Nodes[element.NodesIndexes[0]].R + element.Length / 2) / element.Length,
            _stiffnessTemplate, _stiffnessR);

        return _stiffnessR;
    }

    private Matrix AssembleStiffnessZ(Element element)
    {
        Matrix.Multiply(1d / element.Height,
            _stiffnessTemplate, _stiffnessZ);

        return _stiffnessZ;
    }

    private Matrix AssembleMassR(Element element)
    {
        Matrix.Multiply(Math.Pow(element.Length, 2) / 12d,
            _massRTemplate, _massR);

        Matrix.Multiply(element.Length * _grid.Nodes[element.NodesIndexes[0]].R / 6d,
            _massTemplate, _massZ);

        Matrix.Sum(_massR, _massZ, _massR);

        return _massR;
    }

    private Matrix AssembleMassZ(Element element)
    {
        Matrix.Multiply(element.Height / 6d,
            _massTemplate, _massZ);

        return _massZ;
    }

    private int GetMuIndex(int i)
    {
        return i % 2;
    }

    private int GetNuIndex(int i)
    {
        return i / 2;
    }
}