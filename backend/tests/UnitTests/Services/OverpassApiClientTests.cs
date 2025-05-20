using System.Net;
using System.Text;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using Moq;
using Moq.Protected;
using Services.Implementation.OSM;
using Services.Map;

namespace UnitTests.Services
{
    public class OverpassApiClientTests
    {
        const string Host = "test.host.local";
        const string HostUrl = $"https://{Host}";
        
        [Fact]
        public async Task GetBuildingInformation_ShouldReturnError_WhenNetworkOrServerError()
        {
            // Arrange
            var queryAddress = new Address(city: "Казань", streetName: "Кремлёвская", streetType: "улица", houseNumber: "35");
            var expectedError = MapProviderErrors.GlobalError;
            
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Throws<HttpRequestException>();
            
            var httpClient = new HttpClient(handlerMock.Object);
            IMapProviderService service = new OverpassApiClient(HostUrl, httpClient);
            
            // Act
            var result = await service.GetBuildingInformationAsync(queryAddress, CancellationToken.None);
            
            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(expectedError, result.Error);
        }
        
        [Fact]
        public async Task GetBuildingInformation_ShouldReturnFailure_WhenNothingFound()
        {
            // Arrange
            var queryAddress = new Address(city: "Казань", streetName: "Кремлёвская", streetType: "улица", houseNumber: "35");
            var expectedError = MapProviderErrors.BuildingNotFoundError;
            
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.RequestUri != null &&
                        r.RequestUri.Host == Host &&
                        r.RequestUri.ToString().StartsWith($"{HostUrl}/api/interpreter") &&
                        (r.Method == HttpMethod.Get || r.Method == HttpMethod.Post)
                    ),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        "{\n    \"version\": 0.6,\n    \"generator\": \"Overpass API 0.7.61.8 b1080abd\",\n    \"osm3s\": {\n        \"timestamp_osm_base\": \"2025-05-03T08:23:28Z\",\n        \"timestamp_areas_base\": \"2025-05-03T06:42:40Z\",\n        \"copyright\": \"The data included in this document is from www.openstreetmap.org. The data is made available under ODbL.\"\n    },\n    \"elements\": []\n}",
                        Encoding.UTF8,
                        "application/json"
                    )
                })
                .Verifiable();
            var httpClient = new HttpClient(handlerMock.Object);
            IMapProviderService service = new OverpassApiClient(HostUrl, httpClient);

            // Act
            var informationResult = await service.GetBuildingInformationAsync(queryAddress, CancellationToken.None);
            
            // Assert
            Assert.True(informationResult.IsFailure);
            Assert.Equal(expectedError, informationResult.Error);
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }
        
        [Fact]
        public async Task GetBuildingInformation_ShouldReturnCorrectBuilding()
        {
            // Arrange
            var expectedAddress = new Address(city: "Казань", streetName: "Кремлёвская", streetType: "улица", houseNumber: "35");
            const uint expectedLevelsCount = 17;
            LineString[] expectedCoordinates =
            [
                new ([
                    new Position(latitude:55.7921943, longitude: 49.1217258),
                    new Position(latitude:55.7919225, longitude: 49.1223946),
                    new Position(latitude:55.7920764, longitude: 49.1225962),
                    new Position(latitude:55.7921943, longitude: 49.1217258),
                ])
            ];

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.RequestUri != null &&
                        r.RequestUri.Host == Host &&
                        r.RequestUri.ToString().StartsWith($"{HostUrl}/api/interpreter") &&
                        (r.Method == HttpMethod.Get || r.Method == HttpMethod.Post)
                    ),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        @"{
                            ""version"": 0.6,
                            ""generator"": ""Overpass API 0.7.61.8 b1080abd"",
                            ""osm3s"": {
                                ""timestamp_osm_base"": ""2025-05-02T08:45:25Z"",
                                ""timestamp_areas_base"": ""2025-05-02T07:18:17Z"",
                                ""copyright"": ""The data included in this document is from www.openstreetmap.org. The data is made available under ODbL.""
                            },
                            ""elements"": [
                                {
                                    ""type"": ""way"",
                                    ""id"": 83087730,
                                    ""bounds"": {
                                        ""minlat"": 55.7919225,
                                        ""minlon"": 49.1216524,
                                        ""maxlat"": 55.7928955,
                                        ""maxlon"": 49.1230320
                                    },
                                    ""nodes"": [
                                        966680054, 1148615646, 1148615582, 966680074, 12669675686, 12669675688,
                                        12669675685, 12669675687, 12669675680, 12669675682, 12669675684,
                                        12669675681, 12669675683, 12669675679, 966680066, 12669675678, 966680054
                                    ],
                                    ""geometry"": [
                                        { ""lat"": 55.7921943, ""lon"": 49.1217258 },
                                        { ""lat"": 55.7919225, ""lon"": 49.1223946 },
                                        { ""lat"": 55.7920764, ""lon"": 49.1225962 },
                                        { ""lat"": 55.7921943, ""lon"": 49.1217258 }
                                    ],
                                    ""tags"": {
                                        ""addr:city"": ""Казань"",
                                        ""addr:country"": ""RU"",
                                        ""addr:housenumber"": ""35"",
                                        ""addr:street"": ""Кремлёвская улица"",
                                        ""building"": ""university"",
                                        ""building:levels"": ""17"",
                                        ""height"": ""81"",
                                        ""name"": ""2-й корпус КФУ"",
                                        ""start_date"": ""1977""
                                    }
                                }
                            ]
                        }",
                        Encoding.UTF8,
                        "application/json"
                    )
                })
                .Verifiable();
            var httpClient = new HttpClient(handlerMock.Object);
            IMapProviderService service = new OverpassApiClient(HostUrl, httpClient);

            // Act
            var informationResult = await service.GetBuildingInformationAsync(expectedAddress, CancellationToken.None);
            var information = informationResult.Value;

            // Assert
            Assert.True(informationResult.IsSuccess);
            Assert.NotNull(information);
            Assert.Equal(expectedAddress, information.Address);
            Assert.Equal(expectedLevelsCount, information.LevelsCount);
            Helpers.AssertGeometryEquality(expectedCoordinates.Unpack(), information.Geometry.Coordinates.Unpack());

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }
        
        [Fact]
        public async Task GetBuildingInformation_ShouldSetDefaultLevel_WhenNoTags()
        {
            // Arrange
            var expectedAddress = new Address(city: "Казань", streetName: "Кремлёвская", streetType: "улица", houseNumber: "35");
            const uint expectedLevelsCount = 1;
            LineString[] expectedCoordinates =
            [
                new LineString([
                    new Position(latitude:55.7921943, longitude:49.1217258),
                    new Position(latitude:55.7919225, longitude:49.1223946),
                    new Position(latitude:55.7920764, longitude:49.1225962),
                    new Position(latitude:55.7921943, longitude:49.1217258),
                ])
            ];

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.RequestUri != null &&
                        r.RequestUri.Host == Host &&
                        r.RequestUri.ToString().StartsWith($"{HostUrl}/api/interpreter") &&
                        (r.Method == HttpMethod.Get || r.Method == HttpMethod.Post)
                    ),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        @"{
                            ""version"": 0.6,
                            ""generator"": ""Overpass API 0.7.61.8 b1080abd"",
                            ""osm3s"": {
                                ""timestamp_osm_base"": ""2025-05-02T08:45:25Z"",
                                ""timestamp_areas_base"": ""2025-05-02T07:18:17Z"",
                                ""copyright"": ""The data included in this document is from www.openstreetmap.org. The data is made available under ODbL.""
                            },
                            ""elements"": [
                                {
                                    ""type"": ""way"",
                                    ""id"": 83087730,
                                    ""bounds"": {
                                        ""minlat"": 55.7919225,
                                        ""minlon"": 49.1216524,
                                        ""maxlat"": 55.7928955,
                                        ""maxlon"": 49.1230320
                                    },
                                    ""nodes"": [
                                        966680054, 1148615646, 1148615582, 966680074, 12669675686, 12669675688,
                                        12669675685, 12669675687, 12669675680, 12669675682, 12669675684,
                                        12669675681, 12669675683, 12669675679, 966680066, 12669675678, 966680054
                                    ],
                                    ""geometry"": [
                                        { ""lat"": 55.7921943, ""lon"": 49.1217258 },
                                        { ""lat"": 55.7919225, ""lon"": 49.1223946 },
                                        { ""lat"": 55.7920764, ""lon"": 49.1225962 },
                                        { ""lat"": 55.7921943, ""lon"": 49.1217258 }
                                    ]
                                }
                            ]
                        }",
                        Encoding.UTF8,
                        "application/json"
                    )
                })
                .Verifiable();
            var httpClient = new HttpClient(handlerMock.Object);
            IMapProviderService service = new OverpassApiClient(HostUrl, httpClient);

            // Act
            var informationResult = await service.GetBuildingInformationAsync(expectedAddress, CancellationToken.None);
            var information = informationResult.Value;

            // Assert
            Assert.True(informationResult.IsSuccess);
            Assert.NotNull(information);
            Assert.Equal(expectedAddress, information.Address);
            Assert.Equal(expectedLevelsCount, information.LevelsCount);
            Helpers.AssertGeometryEquality(expectedCoordinates.Unpack(), information.Geometry.Coordinates.Unpack());

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}