using CourseProjectDima.Core.GridComponents;

namespace CourseProjectDima.TwoDimensional.Parameters;

public class MaterialRepository
{
    private readonly Dictionary<int, Material> _materials;

    public MaterialRepository(List<double> lambdas, List<double> sigmas)
    {
        _materials = new Dictionary<int, Material>(lambdas.Count);

        for (var i = 0; i < lambdas.Count; i++)
        {
            _materials.Add(i, new Material(lambdas[i], sigmas[i]));
        }
    }

    public Material GetById(int id)
    {
        return _materials[id];
    }
}