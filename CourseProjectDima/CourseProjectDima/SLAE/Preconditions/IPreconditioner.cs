namespace CourseProjectDima.SLAE.Preconditions;

public interface IPreconditioner<TMatrix>
{
    public TMatrix Decompose(TMatrix globalMatrix);
}