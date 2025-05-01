using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Services.Map;

namespace Services.Implementation.OSM;

public class OverpassApiClient : IMapProviderService
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private string OverpassApiHost { get; }
    private HttpClient Http { get; }
    
    public OverpassApiClient([StringSyntax(StringSyntaxAttribute.Uri)] string host, HttpClient client)
    {
        ArgumentException.ThrowIfNullOrEmpty(host, nameof(host));
        
        OverpassApiHost = host;
        Http = client ?? throw new ArgumentNullException(nameof(client));
    }
    
    public Task<Dictionary<string, object>> GetBuildingInformationAsync(
        string city, 
        string street, 
        string houseNumber, 
        CancellationToken cancellationToken)
    {
        // todo: распарсить улицу и тип улицы. распарсить строение и тип строения
        // todo: сделать запрос к оверпас апи
        throw new NotImplementedException();
    }

    private string ConstructBuildingFetchQuery(string city, 
        string streetType, 
        string streetName, 
        string houseNumber, 
        string unitNumber)
    {
        using var sb = new ValueStringBuilder();
        foreach (var letter in streetName)
        {
            if (letter == 'ё' || letter == 'е')
            {
                sb.Append("(е|ё)");
            }
            else
            {
                sb.Append(letter);
            }
        }
        streetName = sb.ToString();
        
        var query = 
            $"""
            [out:json];
            area[place=city][name="{city}"]->.searchCityArea;
            way[building]["addr:street"~"^({streetName} {streetType}|{streetType} {streetName})$", i]["addr:housenumber"~"^({houseNumber}[[:space:]]*{unitNumber})$"i](area.searchCityArea);
            out body geom;
            """;

        return query;
    }
    
    private ref struct ValueStringBuilder
    {
        private int _bufferPosition;
        private Span<char> _buffer;
        private char[]? _arrayFromPool;

        public ValueStringBuilder()
        {
            _bufferPosition = 0;
            _buffer = new char[32];
            _arrayFromPool = null;
        }

        public ref char this[int index] => ref _buffer[index];

        public void Append(char c)
        {
            if (_bufferPosition == _buffer.Length - 1)
            {
                Grow();
            }

            _buffer[_bufferPosition++] = c;
        }

        public void Append(ReadOnlySpan<char> str)
        {
            var newSize = str.Length + _bufferPosition;
            if (newSize > _buffer.Length)
                Grow(newSize * 2);

            str.CopyTo(_buffer[_bufferPosition..]);
            _bufferPosition += str.Length;
        }

        public override string ToString() => new(_buffer[.._bufferPosition]);

        public void Dispose()
        {
            if (_arrayFromPool is not null)
            {
                ArrayPool<char>.Shared.Return(_arrayFromPool);
            }
        }

        private void Grow(int capacity = 0)
        {
            var currentSize = _buffer.Length;
            var newSize = capacity > 0 ? capacity : currentSize * 2;
            var rented = ArrayPool<char>.Shared.Rent(newSize);
            var oldBuffer = _arrayFromPool;
            _buffer.CopyTo(rented);
            _buffer = _arrayFromPool = rented;
            if (oldBuffer is not null)
            {
                ArrayPool<char>.Shared.Return(oldBuffer);
            }
        }
    }
}