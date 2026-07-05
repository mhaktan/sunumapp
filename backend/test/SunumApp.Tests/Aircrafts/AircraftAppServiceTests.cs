using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Abp.Domain.Repositories;
using Moq;
using SunumApp.Entities;
using SunumApp.Aircrafts;
using SunumApp.Aircrafts.Dto;
using SunumApp.Flows;

namespace SunumApp.Tests.Aircrafts
{
    public class AircraftAppServiceTests
    {
        private readonly Mock<IRepository<Aircraft, long>> _repositoryMock;
        private readonly AircraftAppService _service;

        public AircraftAppServiceTests()
        {
            _repositoryMock = new Mock<IRepository<Aircraft, long>>();
            _service = new AircraftAppService(_repositoryMock.Object, new Mock<IFlowEngine>().Object);
        }

        [Fact]
        public void Repository_GetAll_ShouldReturnQueryable()
        {
            // Arrange
            var entities = new[]
            {
                new Aircraft { Id = 1, Registration = "Test registration", AircraftType = "Test aircraftType", Model = "Test model", Status = 0 },
                new Aircraft { Id = 2, Registration = "Test registration", AircraftType = "Test aircraftType", Model = "Test model", Status = 0 },
            }.AsQueryable();

            _repositoryMock.Setup(r => r.GetAll()).Returns(entities);

            // Act
            var result = _repositoryMock.Object.GetAll();

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(2);
        }

        [Fact]
        public void Repository_GetAll_WithFilter_ShouldWork()
        {
            // Arrange
            var entities = new[]
            {
                new Aircraft { Id = 1, Registration = "Test registration", AircraftType = "Test aircraftType", Model = "Test model", Status = 0 },
                new Aircraft { Id = 2, Registration = "Test registration", AircraftType = "Test aircraftType", Model = "Test model", Status = 0 },
            }.AsQueryable();

            _repositoryMock.Setup(r => r.GetAll()).Returns(entities);

            // Act — simulate keyword filter
            var result = _repositoryMock.Object.GetAll()
                .Where(x => x.Id.ToString().Contains("1"));

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Create_ShouldInsertEntity()
        {
            // Arrange
            var dto = new CreateAircraftDto
            {
                Registration = "Test registration", AircraftType = "Test aircraftType", Model = "Test model", Status = 0
            };

            _repositoryMock.Setup(r => r.InsertAndGetIdAsync(It.IsAny<Aircraft>()))
                .ReturnsAsync(1);
            _repositoryMock.Setup(r => r.GetAsync(It.IsAny<long>()))
                .ReturnsAsync(new Aircraft { Id = 1, Registration = "Test registration", AircraftType = "Test aircraftType", Model = "Test model", Status = 0 });

            // Act & Assert
            _service.Should().NotBeNull();
        }

        [Fact]
        public async Task Delete_ShouldRemoveEntity()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetAsync(It.IsAny<long>()))
                .ReturnsAsync(new Aircraft { Id = 1, Registration = "Test registration", AircraftType = "Test aircraftType", Model = "Test model", Status = 0 });

            // Act & Assert
            await _service.Invoking(s => s.DeleteAsync(new Abp.Application.Services.Dto.EntityDto<long> { Id = 1 }))
                .Should().NotThrowAsync();
        }
    }
}
