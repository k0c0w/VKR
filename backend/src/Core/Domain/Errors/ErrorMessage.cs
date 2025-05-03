namespace Domain.Errors;

public record ErrorMessage
{
    private string Message { get; set; }

    public ErrorMessage(string msg)
    {
        ArgumentException.ThrowIfNullOrEmpty(msg);
        Message = msg;
    }

    public override string ToString() => Message;
    
    public static implicit operator string(ErrorMessage err) => err.ToString();
}