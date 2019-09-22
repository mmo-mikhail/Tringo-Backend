using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using Tringo.FlightsService;
using Tringo.FlightsService.DTO;
using Tringo.WebApp.Controllers;
using Xunit;

namespace Tringo.WebApp.Tests
{
    public class AirportsControllerTest
    {
        [Theory]
        [InlineAutoMoqData(null)]
        [InlineAutoMoqData("")]
        [InlineAutoMoqData("   ")]
        public void GetDestinationPrices_BadRequest(
            string airportCode,
            AirportsController airportsController
        )
        {
            // Act
            var result = airportsController.GetAirportCoordinates(airportCode);

            // Assert
            result.Should().NotBeNull();
            result.Value.Should().BeNull();
            result.Result.Should().BeOfType<BadRequestResult>();
            ((StatusCodeResult)result.Result).StatusCode.Should().Be(400);
        }

        [Theory]
        [InlineAutoMoqData("ADL")]
        public void GetDestinationPrices_NotFound(
            string airportCode,
            [Frozen] Mock<IAirportsService> airportsService,
            AirportsController airportsController)
        {
            // Setup: airports in backend: SYD, MEL. request coming for ADL (adelaide) which is missing
            // Arrange
            var airports = new List<AirportDto>()
            {
                new AirportDto(){ IataCode = "SYD" },
                new AirportDto(){ IataCode = "MEL" }
            };
            airportsService.Setup(_ => _.GetAirports()).Returns(airports);

            // Act
            var result = airportsController.GetAirportCoordinates(airportCode);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<NoContentResult>();
            ((NoContentResult)result.Result).StatusCode.Should().Be(204);
        }

        [Theory]
        [InlineAutoMoqData("SYD")]
        public void GetDestinationPrices_Found_Success(
            string airportCode,
            double expectedLat,
            double expectedLng,
            [Frozen] Mock<IAirportsService> airportsService,
            AirportsController airportsController)
        {
            // Setup: airports in backend: SYD, MEL. request coming for SYD (adelaide) which exists
            // Arrange
            var airports = new List<AirportDto>()
            {
                new AirportDto(){ IataCode = "SYD", Lat = expectedLat, Lng = expectedLng },
                new AirportDto(){ IataCode = "MEL" }
            };
            airportsService.Setup(_ => _.GetAirports()).Returns(airports);

            // Act
            var result = airportsController.GetAirportCoordinates(airportCode);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result.Result).StatusCode.Should().Be(200);

            var okResultObj = ((OkObjectResult)result.Result).Value;
            okResultObj.Should().BeOfType<Coordinates>();

            var resCoord = okResultObj as Coordinates;
            resCoord.Lat.Should().Be(expectedLat);
            resCoord.Lng.Should().Be(expectedLng);
        }
    }
}
