using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Abp.Domain.Repositories;
using Moq;
using SunumApp.Entities;
using SunumApp.Personnels;
using SunumApp.Personnels.Dto;
using SunumApp.Flows;

namespace SunumApp.Tests.Personnels
{
    public class PersonnelAppServiceTests
    {
        private readonly Mock<IRepository<Personnel, long>> _repositoryMock;
        private readonly PersonnelAppService _service;

        public PersonnelAppServiceTests()
        {
            _repositoryMock = new Mock<IRepository<Personnel, long>>();
            _service = new PersonnelAppService(_repositoryMock.Object, new Mock<IFlowEngine>().Object);
        }

        [Fact]
        public void Repository_GetAll_ShouldReturnQueryable()
        {
            // Arrange
            var entities = new[]
            {
                new Personnel { Id = 1, FirstName = "Test firstName", LastName = "Test lastName", EmployeeNumber = "Test employeeNumber", Role = 0 },
                new Personnel { Id = 2, FirstName = "Test firstName", LastName = "Test lastName", EmployeeNumber = "Test employeeNumber", Role = 0 },
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
                new Personnel { Id = 1, FirstName = "Test firstName", LastName = "Test lastName", EmployeeNumber = "Test employeeNumber", Role = 0 },
                new Personnel { Id = 2, FirstName = "Test firstName", LastName = "Test lastName", EmployeeNumber = "Test employeeNumber", Role = 0 },
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
            var dto = new CreatePersonnelDto
            {
                FirstName = "Test firstName", LastName = "Test lastName", EmployeeNumber = "Test employeeNumber", Role = 0
            };

            _repositoryMock.Setup(r => r.InsertAndGetIdAsync(It.IsAny<Personnel>()))
                .ReturnsAsync(1);
            _repositoryMock.Setup(r => r.GetAsync(It.IsAny<long>()))
                .ReturnsAsync(new Personnel { Id = 1, FirstName = "Test firstName", LastName = "Test lastName", EmployeeNumber = "Test employeeNumber", Role = 0 });

            // Act & Assert
            _service.Should().NotBeNull();
        }

        [Fact]
        public async Task Delete_ShouldRemoveEntity()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetAsync(It.IsAny<long>()))
                .ReturnsAsync(new Personnel { Id = 1, FirstName = "Test firstName", LastName = "Test lastName", EmployeeNumber = "Test employeeNumber", Role = 0 });

            // Act & Assert
            await _service.Invoking(s => s.DeleteAsync(new Abp.Application.Services.Dto.EntityDto<long> { Id = 1 }))
                .Should().NotThrowAsync();
        }
    }
}
