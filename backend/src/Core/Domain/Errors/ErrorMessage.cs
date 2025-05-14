namespace Domain.Errors;

public readonly struct ErrorMessage : IEquatable<ErrorMessage>
{
    private string Message { get; init; }

    public ErrorMessage(string msg)
    {
        ArgumentException.ThrowIfNullOrEmpty(msg);
        Message = msg;
    }

    public override bool Equals(object? obj)
    {
        if (obj is ErrorMessage errorMessage)
        {
            return errorMessage == this;
        }
        
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return Message.GetHashCode();
    }

    public override string ToString() => Message;

    public bool Equals(ErrorMessage other)
    {
        return Message == other.Message;
    }
    
    public static bool operator ==(ErrorMessage a, ErrorMessage b) => 
        a.Message == b.Message;

    public static bool operator !=(ErrorMessage a, ErrorMessage b) => !(a == b);
    
    public static implicit operator string(ErrorMessage err) => err.ToString();

    public static readonly ErrorMessage EntityNotfoundError = new("Сущность не найдена.");
    
    public static readonly ErrorMessage EntityIsAlreadyExists = new("Сущность уже существует.");
    
    public static readonly ErrorMessage AbstractError = new("Непредвиденная ошибка.");

    public static class RepositorySpecificErrors
    {
        public static readonly ErrorMessage AddError = new("Возникла ошибка при добавлении в хранилище.");
        
        public static readonly ErrorMessage GetError = new("Возникла ошибка при получении сущности из хранилища.");
        
        public static readonly ErrorMessage TransactionAborted = new("Операция была отменена.");
    }
}