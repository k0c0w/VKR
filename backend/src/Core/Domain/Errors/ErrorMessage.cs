namespace Domain.Errors;

public readonly struct ErrorMessage : IEquatable<ErrorMessage>
{
    public ErrorType Type { get; init; }
    private string Message { get; init; }

    internal ErrorMessage(ErrorType type, string msg)
    {
        ArgumentException.ThrowIfNullOrEmpty(msg);
        Message = msg;
        Type = type;
    }
    
    public override string ToString() => Message;

    public bool Equals(ErrorMessage other)
        => Message == other.Message && Type == other.Type;
    
    public static implicit operator string(ErrorMessage err) => err.ToString();

    public static ErrorMessage DomainError(string message) => new (ErrorType.DomainActionError, message);
    
    public static ErrorMessage ValidationError(string message) => new (ErrorType.ValidationError, message);
    
    public static ErrorMessage SystemError(string message) => new (ErrorType.SystemError, message);
    
    public enum ErrorType : short
    {
        ValidationError = 1,
        DomainActionError,
        SystemError
    }

    public static readonly ErrorMessage EntityNotfoundError = DomainError("Сущность не найдена.");
    
    public static readonly ErrorMessage EntityIsAlreadyExists = DomainError("Сущность уже существует.");
    
    public static readonly ErrorMessage AbstractError = SystemError("Непредвиденная ошибка.");
    
    public static class RepositorySpecificErrors
    {
        public static readonly ErrorMessage AddError = DomainError("Возникла ошибка при добавлении в хранилище.");
        
        public static readonly ErrorMessage GetError = DomainError("Возникла ошибка при получении сущности из хранилища.");
        
        public static readonly ErrorMessage TransactionAborted = DomainError("Операция была отменена.");
    }

    public static class AddressErrors
    {
        public static readonly ErrorMessage CanNotParseStreet = ValidationError("Не удалось распарсить улицу.");
        
        public static readonly ErrorMessage CanNotParseHouse = ValidationError("Не удалось распарсить дом.");
    }

    public static class ItEquipmentCatalogueErrors
    {
        public static readonly ErrorMessage BuildingIsNotPresentInCatalogue =
            DomainError("Данное здание не прдеставлено в каталоге ИТ-оборудования.");
        
        public static readonly ErrorMessage CanNotFetchDataFromCatalogue = DomainError("Не удалось получить данные из каталога ИТ-оборудования.");
    }
    
    public static class AuthenticationErrors
    {
        public static readonly ErrorMessage Unauthorized = DomainError("Запрос не авторизован.");
        public static readonly ErrorMessage AccessDenied = DomainError("Доступ запрещён.");
    }
}