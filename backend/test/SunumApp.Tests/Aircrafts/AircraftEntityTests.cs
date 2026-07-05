using System;
using Xunit;
using FluentAssertions;
using SunumApp.Entities;

namespace SunumApp.Tests.Aircrafts
{
    public class AircraftEntityTests
    {
        [Fact]
        public void Aircraft_ShouldBeCreatable()
        {
            // Act
            var entity = new Aircraft();

            // Assert
            entity.Should().NotBeNull();
        }

        [Fact]
        public void Aircraft_ShouldHaveDefaultValues()
        {
            // Act
            var entity = new Aircraft();

            // Assert
            entity.Id.Should().Be(default(long));

        }

        [Fact]
        public void Aircraft_Registration_ShouldAcceptValue()
        {
            var entity = new Aircraft { Registration = "Test Value" };
            entity.Registration.Should().Be("Test Value");
        }

        [Fact]
        public void Aircraft_AircraftType_ShouldAcceptValue()
        {
            var entity = new Aircraft { AircraftType = "Test Value" };
            entity.AircraftType.Should().Be("Test Value");
        }

        [Fact]
        public void Aircraft_Model_ShouldAcceptValue()
        {
            var entity = new Aircraft { Model = "Test Value" };
            entity.Model.Should().Be("Test Value");
        }

    }
}
