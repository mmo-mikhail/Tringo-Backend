using AutoFixture.Xunit2;
using System;
using System.Collections.Generic;
using System.Net;
using Tringo.FlightsService;
using Tringo.FlightsService.DTO;
using Tringo.WebApp.Controllers;
using Xunit;
using Moq;
using FluentAssertions;
using Tringo.WebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace Tringo.WebApp.Tests
{
    public class FlightsControllerTest
    {
        [Theory]
        [InlineAutoMoqData(null)]
        public void GetDestinationPrices_BadRequest(
            FlightDestinationRequest request,
            FlightsController flightsController
            )
        {
            // Act
            var result = flightsController.GetDestinationPrices(request).Result;

            // Assert
            result.Should().NotBeNull();
            result.Value.Should().BeNull();
            result.Result.Should().BeOfType<BadRequestResult>();
            ( (StatusCodeResult) result.Result).StatusCode.Should().Be(400);
        }

        [Theory]
        [AutoMoqData]
        public void GetDestinationPrices_Success(
            [Frozen]Mock<IFlightsService> flightsService,
            FlightsController flightsController
            )
        {

            // Arrange
            //var request = new Fixture().Create<FlightDestinationRequest>();
            var request = new FlightDestinationRequest
            {
                DepartureAirportId = "MEL",
                Dates = new DatesRequest
                {
                    DateFrom = DateTime.Parse("2019-09-15"),
                    DateUntil = DateTime.Parse("2019-10-15")
                },
                Budget = new Budget() { Min = 0, Max = 1000 },
                SearchArea = new SearchArea()
                {
                    Nw = new Coordinates(43,-32),
                    Se = new Coordinates(32,36)
                }
            };

            var airports = new List<AirportDto>()
            {
                new AirportDto
                {
                    // TODO ...
                }
            };
            var flights = new List<ReturnFlightDestinationDto>()
            {
                new ReturnFlightDestinationDto
                {
                    // TODO ...
                }
            };
            flightsService.Setup(_ => _.GetFlights(It.IsAny<string>())).Returns(flights);
            flightsService.Setup(_ => _.GetAirports()).Returns(airports);

            // Act
            var result = flightsController.GetDestinationPrices(request).Result;

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>();
        }
    }
}
