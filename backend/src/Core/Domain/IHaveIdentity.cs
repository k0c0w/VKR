namespace Domain;

public interface IHaveIdentity<out TIdType> where TIdType : IEquatable<TIdType> 
{
    public TIdType Id { get; }
}