using Xunit;
using NexaCRM.WebClient.Models;
using NexaCRM.WebClient.Models.Enums;
using NexaCRM.WebClient.Services.Mock;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace NexaCRM.WebClient.UnitTests
{
    public class CalendarConflictDetectionTests
    {
        private readonly MockSalesManagementService _salesService;
        private readonly string _testUserId = "testuser1";

        public CalendarConflictDetectionTests()
        {
            _salesService = new MockSalesManagementService(new List<SalesAppointment>());
        }

        [Fact]
        public async System.Threading.Tasks.Task CheckAppointmentConflictsAsync_NoConflicts_ReturnsEmptyList()
        {
            // Arrange
            var startDateTime = new DateTime(2024, 12, 15, 9, 0, 0);
            var endDateTime = new DateTime(2024, 12, 15, 10, 0, 0);

            // Act
            var conflicts = await _salesService.CheckAppointmentConflictsAsync(_testUserId, startDateTime, endDateTime);

            // Assert
            Assert.NotNull(conflicts);
            Assert.Empty(conflicts);
        }

        [Fact]
        public async System.Threading.Tasks.Task CheckAppointmentConflictsAsync_WithConflicts_ReturnsConflictingAppointments()
        {
            // Arrange
            var existingAppointment = new SalesAppointment
            {
                Id = 0,
                Title = "Existing Meeting",
                StartDateTime = new DateTime(2024, 12, 15, 9, 30, 0),
                EndDateTime = new DateTime(2024, 12, 15, 10, 30, 0),
                UserId = _testUserId,
                Type = AppointmentType.Meeting,
                Status = AppointmentStatus.Scheduled
            };

            // Create the existing appointment
            await _salesService.CreateAppointmentAsync(existingAppointment);

            // Try to create a conflicting appointment
            var conflictStartDateTime = new DateTime(2024, 12, 15, 10, 0, 0);
            var conflictEndDateTime = new DateTime(2024, 12, 15, 11, 0, 0);

            // Act
            var conflicts = await _salesService.CheckAppointmentConflictsAsync(_testUserId, conflictStartDateTime, conflictEndDateTime);

            // Assert
            Assert.NotNull(conflicts);
            Assert.Single(conflicts);
            Assert.Equal("Existing Meeting", conflicts.First().Title);
        }

        [Fact]
        public async System.Threading.Tasks.Task CheckAppointmentConflictsAsync_ExcludeCurrentAppointment_NoConflicts()
        {
            // Arrange
            var appointment = new SalesAppointment
            {
                Id = 0,
                Title = "Test Meeting",
                StartDateTime = new DateTime(2024, 12, 15, 14, 0, 0),
                EndDateTime = new DateTime(2024, 12, 15, 15, 0, 0),
                UserId = _testUserId,
                Type = AppointmentType.Meeting,
                Status = AppointmentStatus.Scheduled
            };

            // Create the appointment
            var createdAppointment = await _salesService.CreateAppointmentAsync(appointment);

            // Check for conflicts when editing the same appointment
            var conflicts = await _salesService.CheckAppointmentConflictsAsync(
                _testUserId, 
                createdAppointment.StartDateTime, 
                createdAppointment.EndDateTime, 
                createdAppointment.Id);

            // Assert
            Assert.NotNull(conflicts);
            Assert.Empty(conflicts);
        }

        [Fact]
        public async System.Threading.Tasks.Task CheckAppointmentConflictsAsync_DifferentUsers_NoConflicts()
        {
            // Arrange
            var user1Appointment = new SalesAppointment
            {
                Id = 0,
                Title = "User 1 Meeting",
                StartDateTime = new DateTime(2024, 12, 15, 16, 0, 0),
                EndDateTime = new DateTime(2024, 12, 15, 17, 0, 0),
                UserId = "user1",
                Type = AppointmentType.Meeting,
                Status = AppointmentStatus.Scheduled
            };

            // Create appointment for user1
            await _salesService.CreateAppointmentAsync(user1Appointment);

            // Check conflicts for a different user at the same time
            var conflicts = await _salesService.CheckAppointmentConflictsAsync(
                "user2", 
                user1Appointment.StartDateTime, 
                user1Appointment.EndDateTime);

            // Assert
            Assert.NotNull(conflicts);
            Assert.Empty(conflicts);
        }

        [Fact]
        public async System.Threading.Tasks.Task CheckAppointmentConflictsAsync_PartialOverlap_ReturnsConflicts()
        {
            // Arrange
            var existingAppointment = new SalesAppointment
            {
                Id = 0,
                Title = "Existing Partial Meeting",
                StartDateTime = new DateTime(2024, 12, 15, 13, 0, 0),
                EndDateTime = new DateTime(2024, 12, 15, 14, 30, 0),
                UserId = _testUserId,
                Type = AppointmentType.Meeting,
                Status = AppointmentStatus.Scheduled
            };

            // Create the existing appointment
            await _salesService.CreateAppointmentAsync(existingAppointment);

            // Try to create a partially overlapping appointment
            var partialOverlapStart = new DateTime(2024, 12, 15, 14, 0, 0);
            var partialOverlapEnd = new DateTime(2024, 12, 15, 15, 0, 0);

            // Act
            var conflicts = await _salesService.CheckAppointmentConflictsAsync(_testUserId, partialOverlapStart, partialOverlapEnd);

            // Assert
            Assert.NotNull(conflicts);
            Assert.Single(conflicts);
            Assert.Equal("Existing Partial Meeting", conflicts.First().Title);
        }
    }
}