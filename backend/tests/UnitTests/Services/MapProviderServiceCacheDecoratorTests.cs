using System.Text.Json;
using Domain;
using Domain.Errors;
using Domain.GeoJson;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using ResultMonad;
using Services.Implementation.OSM;
using Services.Map;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane;
using ZiggyCreatures.Caching.Fusion.Events;
using ZiggyCreatures.Caching.Fusion.Plugins;
using ZiggyCreatures.Caching.Fusion.Serialization;

namespace UnitTests.Services;

public class MapProviderServiceCacheDecoratorTests
{
    [Fact]
    public async Task GetBuildingInformationAsync_ReturnsValueFromCache()
    {
        // Arrange
        var mockOriginalService = new Mock<IMapProviderService>();
        var mockCache = new Mock<IFusionCache>();
        var service = new MapProviderServiceCacheDecorator(mockOriginalService.Object, mockCache.Object);

        var expectedAddress = new Address("Казань", "улица", "Кремлёвская", "35");
        var expectedGeometry = new BuildingGeometry([[new LatLng{ Lat = 1, Lng = 1 }]]);
        var buildingInfo = new BuildingInformation()
        {
            Address = expectedAddress,
            Geometry = expectedGeometry,
            LevelsCount = 1
        };
        var serialized = JsonSerializer.Serialize(buildingInfo);
        var ct = CancellationToken.None;
        
        var successResult = Result.Ok<BuildingInformation, ErrorMessage>(buildingInfo);
        mockOriginalService
            .Setup(s => s.GetBuildingInformationAsync(expectedAddress, ct))
            .ReturnsAsync(successResult);
        mockCache
            .Setup(c => c.GetOrDefaultAsync<string?>(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(serialized);
        mockCache.Setup(c => c.SetAsync(
                It.IsAny<string>(), It.IsAny<BuildingInformation>(),
                It.IsAny<FusionCacheEntryOptions?>(),
                    It.IsAny<IEnumerable<string>?>(),
                ct))
            .Returns(ValueTask.CompletedTask);

        // Act
        var result = await service.GetBuildingInformationAsync(expectedAddress, ct);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(buildingInfo, result.Value);
        mockCache.Verify(c => c.SetAsync(
            It.IsAny<string>(), It.IsAny<BuildingInformation>(),
            It.IsAny<FusionCacheEntryOptions?>(),
            It.IsAny<IEnumerable<string>?>(),
            ct), Times.Never);
        mockCache
            .Verify(c => c.GetOrDefaultAsync<string?>(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()),
                Times.AtLeastOnce());
    }
    
    [Fact]
    public async Task GetBuildingInformationAsync_ReturnsCachedNotFoundError()
    {
        // Arrange
        var mockOriginalService = new Mock<IMapProviderService>();
        var stub = new CacheStub();
        var service = new MapProviderServiceCacheDecorator(mockOriginalService.Object, stub);

        var expectedAddress = new Address("Казань", "улица", "Кремлёвская", "35");
        var ct = CancellationToken.None;
        var notFoundError = MapProviderErrors.BuildingNotFoundError;
        var notFoundErrorMessage = notFoundError.ToString();
        var notFoundResult = Result.Fail<BuildingInformation, ErrorMessage>(notFoundError);
        stub.Cache.Add($"overpass_api:{expectedAddress}", notFoundErrorMessage);
        
        mockOriginalService
            .Setup(s => s.GetBuildingInformationAsync(expectedAddress, ct))
            .ReturnsAsync(notFoundResult);

        // Act
        var result = await service.GetBuildingInformationAsync(expectedAddress, ct);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(notFoundError, result.Error);
        Assert.Equal(0 , stub.SetCallCount);
        Assert.Equal(1 , stub.GetOrDefaultCallCount);
    }

    [Fact]
    public async Task GetBuildingInformationAsync_DoesNotCacheFailures()
    {
        // Arrange
        var mockOriginalService = new Mock<IMapProviderService>();
        var mockCache = new Mock<IFusionCache>();
        var service = new MapProviderServiceCacheDecorator(mockOriginalService.Object, mockCache.Object);

        var expectedAddress = new Address("Казань", "улица", "Кремлёвская", "35");
        var ct = CancellationToken.None;
        var error = new ErrorMessage("Some error");
        var errorResult = Result.Fail<BuildingInformation, ErrorMessage>(error);
        
        mockOriginalService
            .Setup(s => s.GetBuildingInformationAsync(expectedAddress, ct))
            .ReturnsAsync(errorResult);
        mockCache
            .Setup(c => c.GetOrDefaultAsync<string?>(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        mockCache.Setup(c => c.SetAsync(
                It.IsAny<string>(), It.IsAny<BuildingInformation>(),
                It.IsAny<FusionCacheEntryOptions?>(),
                It.IsAny<IEnumerable<string>?>(),
                ct))
            .Returns(ValueTask.CompletedTask);

        // Act
        var result = await service.GetBuildingInformationAsync(expectedAddress, ct);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
        mockCache.Verify(c => c.SetAsync(
            It.IsAny<string>(), It.IsAny<BuildingInformation>(),
            It.IsAny<FusionCacheEntryOptions?>(),
            It.IsAny<IEnumerable<string>?>(),
            ct), Times.Never);
        mockCache
            .Verify(c => c.GetOrDefaultAsync<string?>(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()),
                Times.AtLeastOnce());
    }
    
    [Fact]
    public async Task GetBuildingInformationAsync_CachesBuildingInfo_WhenOriginalServiceSucceedsAndNoBuildingInfoInCache()
    {
        // Arrange
        var mockOriginalService = new Mock<IMapProviderService>();
        var cacheStub = new CacheStub();
        var service = new MapProviderServiceCacheDecorator(mockOriginalService.Object, cacheStub);

        var expectedAddress = new Address("Казань", "улица", "Кремлёвская", "35");
        var expectedGeometry = new BuildingGeometry([[new LatLng{ Lat = 1, Lng = 1 }]]);
        var buildingInfo = new BuildingInformation()
        {
            Address = expectedAddress,
            Geometry = expectedGeometry,
            LevelsCount = 1
        };
        var ct = CancellationToken.None;
    
        var successResult = Result.Ok<BuildingInformation, ErrorMessage>(buildingInfo);
        mockOriginalService
            .Setup(s => s.GetBuildingInformationAsync(expectedAddress, ct))
            .ReturnsAsync(successResult);

        // Act
        var result = await service.GetBuildingInformationAsync(expectedAddress, ct);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(buildingInfo, result.Value);
        mockOriginalService.Verify(s => s.GetBuildingInformationAsync(expectedAddress, ct), Times.Once);
        Assert.Equal(1, cacheStub.GetOrDefaultCallCount);
        Assert.Equal(1, cacheStub.SetCallCount);
    }
    
    private class CacheStub : IFusionCache 
    { 
        public readonly Dictionary<string, object> Cache = new(); 
        public int GetOrDefaultCallCount { get; private set; } 
        public int SetCallCount { get; private set; } 
        
        public FusionCacheEntryOptions CreateEntryOptions(Action<FusionCacheEntryOptions>? setupAction = null, TimeSpan? duration = null)
        {

            var options = new FusionCacheEntryOptions();

            if (setupAction is not null)
            {
                setupAction(options);
                
            }

            return options;
        }

        public ValueTask<TValue> GetOrSetAsync<TValue>(string key, Func<FusionCacheFactoryExecutionContext<TValue>, CancellationToken, Task<TValue>> factory, MaybeValue<TValue> failSafeDefaultValue = new MaybeValue<TValue>(),
            FusionCacheEntryOptions? options = null, IEnumerable<string>? tags = null,
            CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public TValue GetOrSet<TValue>(string key, Func<FusionCacheFactoryExecutionContext<TValue>, CancellationToken, TValue> factory, MaybeValue<TValue> failSafeDefaultValue = new MaybeValue<TValue>(),
            FusionCacheEntryOptions? options = null, IEnumerable<string>? tags = null,
            CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public ValueTask<TValue> GetOrSetAsync<TValue>(string key, TValue defaultValue, FusionCacheEntryOptions? options = null,
            IEnumerable<string>? tags = null, CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public TValue GetOrSet<TValue>(string key, TValue defaultValue, FusionCacheEntryOptions? options = null,
            IEnumerable<string>? tags = null, CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        ValueTask<TValue?> IFusionCache.GetOrDefaultAsync<TValue>(string key, TValue? defaultValue,
            FusionCacheEntryOptions? options, CancellationToken token) where TValue : default
        {
            GetOrDefaultCallCount++;
            if (Cache.TryGetValue(key, out var value) && value is TValue typedValue)
            {
                return ValueTask.FromResult(typedValue);
            }
            return ValueTask.FromResult(defaultValue);
        }

        public TValue? GetOrDefault<TValue>(string key, TValue? defaultValue = default(TValue?),
            FusionCacheEntryOptions? options = null, CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public ValueTask<MaybeValue<TValue>> TryGetAsync<TValue>(string key, FusionCacheEntryOptions? options = null,
            CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public MaybeValue<TValue> TryGet<TValue>(string key, FusionCacheEntryOptions? options = null,
            CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public ValueTask SetAsync<TValue>(string key, TValue value, FusionCacheEntryOptions? options = null, IEnumerable<string>? tags = null,
            CancellationToken token = new CancellationToken())
        {
            SetCallCount++;
            Cache[key] = value;
            return ValueTask.CompletedTask;
        }

        public void Set<TValue>(string key, TValue value, FusionCacheEntryOptions? options = null, IEnumerable<string>? tags = null,
            CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public ValueTask RemoveAsync(string key, FusionCacheEntryOptions? options = null,
            CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public void Remove(string key, FusionCacheEntryOptions? options = null, CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public ValueTask ExpireAsync(string key, FusionCacheEntryOptions? options = null,
            CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public void Expire(string key, FusionCacheEntryOptions? options = null, CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public ValueTask RemoveByTagAsync(string tag, FusionCacheEntryOptions? options = null,
            CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public ValueTask RemoveByTagAsync(IEnumerable<string> tags, FusionCacheEntryOptions? options = null,
            CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public void RemoveByTag(string tag, FusionCacheEntryOptions? options = null,
            CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public void RemoveByTag(IEnumerable<string> tags, FusionCacheEntryOptions? options = null,
            CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public ValueTask ClearAsync(bool allowFailSafe = true, FusionCacheEntryOptions? options = null,
            CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public void Clear(bool allowFailSafe = true, FusionCacheEntryOptions? options = null,
            CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public IFusionCache SetupSerializer(IFusionCacheSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public IFusionCache SetupDistributedCache(IDistributedCache distributedCache)
        {
            throw new NotImplementedException();
        }

        public IFusionCache SetupDistributedCache(IDistributedCache distributedCache, IFusionCacheSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public IFusionCache RemoveDistributedCache()
        {
            throw new NotImplementedException();
        }

        public IFusionCache SetupBackplane(IFusionCacheBackplane backplane)
        {
            throw new NotImplementedException();
        }

        public IFusionCache RemoveBackplane()
        {
            throw new NotImplementedException();
        }

        public void AddPlugin(IFusionCachePlugin plugin)
        {
            throw new NotImplementedException();
        }

        public bool RemovePlugin(IFusionCachePlugin plugin)
        {
            throw new NotImplementedException();
        }

        public string CacheName { get; }
        public string InstanceId { get; }
        public FusionCacheEntryOptions DefaultEntryOptions { get; }
        public bool HasDistributedCache { get; }
        public IDistributedCache? DistributedCache { get; }
        public bool HasBackplane { get; }
        public IFusionCacheBackplane? Backplane { get; }
        public FusionCacheEventsHub Events { get; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
