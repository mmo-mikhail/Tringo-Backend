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
using Microsoft.Extensions.Logging;

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
            ((StatusCodeResult) result.Result).StatusCode.Should().Be(400);
        }

        [Theory]
        [AutoMoqData]
        public void GetDestinationPrices_NoContent(
            [Frozen] Mock<IFlightsService> flightsService,
            [Frozen] Mock<IAirportsService> airportsService,
            FlightsController flightsController
        )
        {
            // Arrange
            //var request = new Fixture().Create<FlightDestinationRequest>();
            var request = new FlightDestinationRequest
            {
                DepartureAirportId = "MEL",
                Dates = new DatesRequest(),
                Budget = new Budget() {Min = 0, Max = 1000},
                SearchArea = new SearchArea()
                {
                    Nw = new Coordinates(43, -32),
                    Se = new Coordinates(32, 36)
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
            flightsService.Setup(_ => _.GetFlights(It.IsAny<WJFlightsRequest>())).ReturnsAsync(flights);
            airportsService.Setup(_ => _.GetAirports()).Returns(airports);

            // Act
            var result = flightsController.GetDestinationPrices(request).Result;

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<NoContentResult>();
            ((NoContentResult) result.Result).StatusCode.Should().Be(204);
        }

        [Theory]
        [AutoMoqData]
        public void GetDestinationPrices_OkResult(
            [Frozen] Mock<IFlightsService> flightsServiceMock,
            [Frozen] Mock<IAirportsService> airportsService,
            [Frozen] Mock<IDestinationsFilter> destinationsFilterMock,
            FlightsController flightsController
        )
        {
            // Arrange
            var request = new FlightDestinationRequest
            {
                DepartureAirportId = "MEL",
                Dates = new DatesRequest(),
                Budget = new Budget() {Min = 0, Max = 1000},
                SearchArea = new SearchArea()
                {
                    Nw = new Coordinates(43, -32),
                    Se = new Coordinates(32, 36)
                }
            };

            var flights = new List<ReturnFlightDestinationDto>()
            {
                new ReturnFlightDestinationDto
                {
                    To = "MEL",
                    LowestPrice = 50,
                    DateDeparture = DateTime.Now,
                    DateBack = DateTime.Now
                }
            };

            var airports = new List<AirportDto>()
            {
                new AirportDto
                {
                    Lat = -5.1912441010878609,
                    Lng = 104.71056084999998,
                    IataCode = "MEL",
                    AirportName = "Tullamarine",
                    RelatedCityName = "Melbourne"
                }
            };

            flightsServiceMock.Setup(_ => _.GetFlights(It.IsAny<WJFlightsRequest>())).ReturnsAsync(flights);
            airportsService.Setup(fs => fs.GetAirports()).Returns(airports);
            destinationsFilterMock.Setup(df => df.FilterAirports(It.IsAny<List<AirportDto>>(), It.IsAny<SearchArea>()))
                .Returns(airports);
            destinationsFilterMock
                .Setup(df => df.FilterFlightsByDates(It.IsAny<List<ReturnFlightDestinationDto>>(), request.Dates))
                .Returns(flights);
            destinationsFilterMock
                .Setup(df => df.FilterLowestPriceOnly(It.IsAny<List<ReturnFlightDestinationDto>>()))
                .Returns<IEnumerable<ReturnFlightDestinationDto>>((sourceAirports) => sourceAirports);

            //            // Act
            var result = flightsController.GetDestinationPrices(request).Result;
//            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult) result.Result).StatusCode.Should().Be(200);
        }
        
        [Theory]
        [AutoMoqData]
        public void GetDestinationPrices_PriceWithinBudget_True(
            [Frozen] Mock<IFlightsService> flightsServiceMock,
            [Frozen] Mock<IAirportsService> airportsService,
            [Frozen] Mock<IDestinationsFilter> destinationsFilterMock,
            FlightsController flightsController
        )
        {
            // Arrange
            var request = new FlightDestinationRequest
            {
                DepartureAirportId = "MEL",
                Dates = new DatesRequest(),
                Budget = new Budget() {Min = 0, Max = 1000},
                SearchArea = new SearchArea()
                {
                    Nw = new Coordinates(43, -32),
                    Se = new Coordinates(32, 36)
                }
            };

            var flights = new List<ReturnFlightDestinationDto>()
            {
                new ReturnFlightDestinationDto
                {
                    To = "MEL",
                    LowestPrice = 50,
                    DateDeparture = DateTime.Now,
                    DateBack = DateTime.Now
                }
            };

            var airports = new List<AirportDto>()
            {
                new AirportDto
                {
                    Lat = -5.1912441010878609,
                    Lng = 104.71056084999998,
                    IataCode = "MEL",
                    AirportName = "Tullamarine",
                    RelatedCityName = "Melbourne"
                }
            };

            flightsServiceMock.Setup(_ => _.GetFlights(It.IsAny<WJFlightsRequest>())).ReturnsAsync(flights);
            airportsService.Setup(fs => fs.GetAirports()).Returns(airports);
            destinationsFilterMock.Setup(df => df.FilterAirports(It.IsAny<List<AirportDto>>(), It.IsAny<SearchArea>()))
                .Returns(airports);
            destinationsFilterMock
                .Setup(df => df.FilterFlightsByDates(It.IsAny<List<ReturnFlightDestinationDto>>(), request.Dates))
                .Returns<IEnumerable<ReturnFlightDestinationDto>, DatesRequest>((sourceAirports, dates) => sourceAirports);
            destinationsFilterMock
                .Setup(df => df.FilterLowestPriceOnly(It.IsAny<List<ReturnFlightDestinationDto>>()))
                .Returns<IEnumerable<ReturnFlightDestinationDto>>((sourceAirports) => sourceAirports);

            // Act
            var result = flightsController.GetDestinationPrices(request).Result;
            var okResult = ((OkObjectResult) result.Result);
            var responseList = okResult.Value as List<FlightDestinationResponse>; 
            
            // Assert
            responseList.Should().NotBeNull();
            responseList.Should().OnlyContain(x => x.Price <= request.Budget.Max);

        }
        
        [Theory]
        [AutoMoqData]
        public void GetDestinationPrices_PriceWithinBudget_False(
            [Frozen] Mock<IFlightsService> flightsServiceMock,
            [Frozen] Mock<IAirportsService> airportsService,
            [Frozen] Mock<IDestinationsFilter> destinationsFilterMock,
            FlightsController flightsController
        )
        {
            // Arrange
            var request = new FlightDestinationRequest
            {
                DepartureAirportId = "MEL",
                Dates = new DatesRequest(),
                Budget = new Budget() {Min = 0, Max = 1000},
                SearchArea = new SearchArea()
                {
                    Nw = new Coordinates(43, -32),
                    Se = new Coordinates(32, 36)
                }
            };

            var flights = new List<ReturnFlightDestinationDto>()
            {
                new ReturnFlightDestinationDto
                {
                    To = "MEL",
                    LowestPrice = 1200,
                    DateDeparture = DateTime.Now,
                    DateBack = DateTime.Now
                }
            };

            var airports = new List<AirportDto>()
            {
                new AirportDto
                {
                    Lat = -5.1912441010878609,
                    Lng = 104.71056084999998,
                    IataCode = "MEL",
                    AirportName = "Tullamarine",
                    RelatedCityName = "Melbourne"
                }
            };

            flightsServiceMock.Setup(_ => _.GetFlights(It.IsAny<WJFlightsRequest>())).ReturnsAsync(flights);
            airportsService.Setup(fs => fs.GetAirports()).Returns(airports);
            destinationsFilterMock.Setup(df => df.FilterAirports(It.IsAny<List<AirportDto>>(), It.IsAny<SearchArea>()))
                .Returns(airports);
            destinationsFilterMock
                .Setup(df => df.FilterFlightsByDates(It.IsAny<List<ReturnFlightDestinationDto>>(), request.Dates))
                .Returns<IEnumerable<ReturnFlightDestinationDto>, DatesRequest>((sourceAirports, dates) => sourceAirports);
            destinationsFilterMock
                .Setup(df => df.FilterLowestPriceOnly(It.IsAny<List<ReturnFlightDestinationDto>>()))
                .Returns<IEnumerable<ReturnFlightDestinationDto>>((sourceAirports) => sourceAirports);

            // Act
            var result = flightsController.GetDestinationPrices(request).Result;

            // Assert
            result.Result.Should().BeOfType<NoContentResult>();
        }
        
        [Theory]
        [AutoMoqData]
        public void GetDestinationPrices_DateDepartureMatchDateFrom_True(
            [Frozen] Mock<IFlightsService> flightsServiceMock,
            [Frozen] Mock<IAirportsService> airportsService,
            [Frozen] Mock<IDestinationsFilter> destinationsFilterMock,
            FlightsController flightsController
        )
        {
            // Arrange
            var request = new FlightDestinationRequest
            {
                DepartureAirportId = "MEL",
                Dates = new DatesRequest
                {
                    MonthIdx = 9
                },
                Budget = new Budget() {Min = 0, Max = 1000},
                SearchArea = new SearchArea()
                {
                    Nw = new Coordinates(43, -32),
                    Se = new Coordinates(32, 36)
                }
            };

            var flights = new List<ReturnFlightDestinationDto>()
            {
                new ReturnFlightDestinationDto
                {
                    To = "MEL",
                    LowestPrice = 50,
                    DateDeparture = DateTime.Parse("2019-09-15"),
                    DateBack = DateTime.Parse("2019-9-25")
                }
            };

            var airports = new List<AirportDto>()
            {
                new AirportDto
                {
                    Lat = -5.1912441010878609,
                    Lng = 104.71056084999998,
                    IataCode = "MEL",
                    AirportName = "Tullamarine",
                    RelatedCityName = "Melbourne"
                }
            };

            flightsServiceMock.Setup(_ => _.GetFlights(It.IsAny<WJFlightsRequest>())).ReturnsAsync(flights);
            airportsService.Setup(fs => fs.GetAirports()).Returns(airports);
            destinationsFilterMock.Setup(df => df.FilterAirports(It.IsAny<List<AirportDto>>(), It.IsAny<SearchArea>()))
                .Returns(airports);
            destinationsFilterMock
                .Setup(df => df.FilterFlightsByDates(It.IsAny<List<ReturnFlightDestinationDto>>(), request.Dates))
                .Returns(flights);
            destinationsFilterMock
                .Setup(df => df.FilterLowestPriceOnly(It.IsAny<List<ReturnFlightDestinationDto>>()))
                .Returns<IEnumerable<ReturnFlightDestinationDto>>((sourceAirports) => sourceAirports);

            // Act
            var result = flightsController.GetDestinationPrices(request).Result;
            var okResult = ((OkObjectResult) result.Result);
            var responseList = okResult.Value as List<FlightDestinationResponse>; 
            
            // Assert
            responseList.Should().NotBeNull();
            responseList.Should().OnlyContain(i => i.FlightDates.DepartureDate.Month == request.Dates.MonthIdx);

        }
        
        [Theory]
        [AutoMoqData]
        public void GetDestinationPrices_DateDepartureMatchDateFrom_False(
            [Frozen] Mock<IFlightsService> flightsServiceMock,
            [Frozen] Mock<IAirportsService> airportsService,
            [Frozen] Mock<IDestinationsFilter> destinationsFilterMock,
            FlightsController flightsController
        )
        {
            // Arrange
            var request = new FlightDestinationRequest
            {
                DepartureAirportId = "MEL",
                Dates = new DatesRequest
                {
                    MonthIdx = 10
                },


                Budget = new Budget() {Min = 0, Max = 1000},
                SearchArea = new SearchArea()
                {
                    Nw = new Coordinates(43, -32),
                    Se = new Coordinates(32, 36)
                }
            };

            var flights = new List<ReturnFlightDestinationDto>()
            {
                new ReturnFlightDestinationDto
                {
                    To = "MEL",
                    LowestPrice = 50,
                    DateDeparture = DateTime.Now,
                    DateBack = DateTime.Now
                }
            };

            var airports = new List<AirportDto>()
            {
                new AirportDto
                {
                    Lat = -5.1912441010878609,
                    Lng = 104.71056084999998,
                    IataCode = "MEL",
                    AirportName = "Tullamarine",
                    RelatedCityName = "Melbourne"
                }
            };

            flightsServiceMock.Setup(_ => _.GetFlights(It.IsAny<WJFlightsRequest>())).ReturnsAsync(flights);
            airportsService.Setup(fs => fs.GetAirports()).Returns(airports);
            destinationsFilterMock.Setup(df => df.FilterAirports(It.IsAny<List<AirportDto>>(), It.IsAny<SearchArea>()))
                .Returns(airports);
            destinationsFilterMock
                .Setup(df => df.FilterFlightsByDates(It.IsAny<List<ReturnFlightDestinationDto>>(), request.Dates))
                .Returns(flights);
            destinationsFilterMock
                .Setup(df => df.FilterLowestPriceOnly(It.IsAny<List<ReturnFlightDestinationDto>>()))
                .Returns<IEnumerable<ReturnFlightDestinationDto>>((sourceAirports) => sourceAirports);

            // Act
            var result = flightsController.GetDestinationPrices(request).Result;
            var okResult = ((OkObjectResult) result.Result);
            var responseList = okResult.Value as List<FlightDestinationResponse>; 
            
            // Assert
            responseList.Should().NotBeNull();
            responseList.Should().NotContain(i => i.FlightDates.DepartureDate.Month == request.Dates.MonthIdx);

        }
        
        [Theory]
        [AutoMoqData]
        public void GetDestinationPrices_FlightDestinationWithinSearchArea_True(
            [Frozen] Mock<IFlightsService> flightsServiceMock,
            [Frozen] Mock<IAirportsService> airportsService,
            [Frozen] Mock<IDestinationsFilter> destinationsFilterMock,
            FlightsController flightsController
        )
        {
            // Arrange
            var request = new FlightDestinationRequest
            {
                DepartureAirportId = "MEL",
                Dates = new DatesRequest
                {
                    MonthIdx = 5
                },
                Budget = new Budget() {Min = 0, Max = 1000},
                SearchArea = new SearchArea()
                {
                    Nw = new Coordinates(43, -32),
                    Se = new Coordinates(32, 36)
                }
            };

            var flights = new List<ReturnFlightDestinationDto>()
            {
                new ReturnFlightDestinationDto
                {
                    To = "MEL",
                    LowestPrice = 50,
                    DateDeparture = DateTime.Now,
                    DateBack = DateTime.Now
                }
            };

            var airports = new List<AirportDto>()
            {
                new AirportDto
                {
                    Lat = -5.1912441010878609,
                    Lng = 104.71056084999998,
                    IataCode = "MEL",
                    AirportName = "Tullamarine",
                    RelatedCityName = "Melbourne"
                }
            };

            flightsServiceMock.Setup(_ => _.GetFlights(It.IsAny<WJFlightsRequest>())).ReturnsAsync(flights);
            airportsService.Setup(fs => fs.GetAirports()).Returns(airports);
            destinationsFilterMock.Setup(df => df.FilterAirports(It.IsAny<List<AirportDto>>(), It.IsAny<SearchArea>()))
                .Returns(airports);
            destinationsFilterMock
                .Setup(df => df.FilterFlightsByDates(It.IsAny<List<ReturnFlightDestinationDto>>(), request.Dates))
                .Returns<IEnumerable<ReturnFlightDestinationDto>, DatesRequest>((sourceAirports, dates) => sourceAirports);
            destinationsFilterMock
                .Setup(df => df.FilterLowestPriceOnly(It.IsAny<List<ReturnFlightDestinationDto>>()))
                .Returns<IEnumerable<ReturnFlightDestinationDto>>((sourceAirports) => sourceAirports);

            // Act
            var result = flightsController.GetDestinationPrices(request).Result;
            var okResult = ((OkObjectResult) result.Result);
            var responseList = okResult.Value as List<FlightDestinationResponse>;

            // Assert
            responseList.Should().NotBeNull();
            responseList.Should().HaveCount(1);
    
        }
        
         [Theory]
        [AutoMoqData]
        public void GetDestinationPrices_FlightDestinationWithinSearchArea_False(
            [Frozen] Mock<IFlightsService> flightsServiceMock,
            [Frozen] Mock<IAirportsService> airportsService,
            [Frozen] Mock<IDestinationsFilter> destinationsFilterMock,
            FlightsController flightsController
        )
        {
            // Arrange
            var request = new FlightDestinationRequest
            {
                DepartureAirportId = "MEL",
                Dates = new DatesRequest
                {
                    MonthIdx = 5
                },
                Budget = new Budget() {Min = 0, Max = 1000},
                SearchArea = new SearchArea()
                {
                    Nw = new Coordinates(6, -7),
                    Se = new Coordinates(12, 11)
                }
            };

            var flights = new List<ReturnFlightDestinationDto>()
            {
                new ReturnFlightDestinationDto
                {
                    To = "GOH",
                    LowestPrice = 50,
                    DateDeparture = DateTime.Now,
                    DateBack = DateTime.Now
                }
            };

            var airports = new List<AirportDto>()
            {
                new AirportDto
                {
                    Lat = 64.187832582,
                    Lng = -51.673497306,
                    IataCode = "GOH",
                    AirportName = "Nuuk Airport",
                    RelatedCityName = "Nuuk"

                }
            };

            flightsServiceMock.Setup(_ => _.GetFlights(It.IsAny<WJFlightsRequest>())).ReturnsAsync(flights);
            airportsService.Setup(fs => fs.GetAirports()).Returns(airports);
            destinationsFilterMock.Setup(df => df.FilterAirports(It.IsAny<List<AirportDto>>(), It.IsAny<SearchArea>()))
                .Returns(airports);
            destinationsFilterMock
                .Setup(df => df.FilterFlightsByDates(It.IsAny<List<ReturnFlightDestinationDto>>(), request.Dates))
                .Returns<IEnumerable<ReturnFlightDestinationDto>, DatesRequest>((sourceAirports, dates) => sourceAirports);
            destinationsFilterMock
                .Setup(df => df.FilterLowestPriceOnly(It.IsAny<List<ReturnFlightDestinationDto>>()))
                .Returns<IEnumerable<ReturnFlightDestinationDto>>((sourceAirports) => sourceAirports);

            // Act
            var result = flightsController.GetDestinationPrices(request).Result;
            var okResult = ((OkObjectResult) result.Result);
            var responseList = okResult.Value as List<FlightDestinationResponse>;

            // Assert
            responseList.Should().NotBeNull();
            responseList.Should().HaveCount(1);
    
        }
    }
}
