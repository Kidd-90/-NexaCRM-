using System;

namespace NexaCRM.Services.Admin.Interfaces
{
    public interface IDedupeConfigService
    {
        bool Enabled { get; set; }
        int Days { get; set; }
        bool IncludeFuzzy { get; set; }
        int ScoreThreshold { get; set; }
        int MonitorIntervalMinutes { get; set; }
        bool NotifyOnSameCount { get; set; }

        // Optional field usage toggles
        bool UseGender { get; set; }
        bool UseAddress { get; set; }
        bool UseJobTitle { get; set; }
        bool UseMaritalStatus { get; set; }
        bool UseProofNumber { get; set; }
        bool UseDbPrice { get; set; }
        bool UseHeadquarters { get; set; }
        bool UseInsuranceName { get; set; }
        bool UseCarJoinDate { get; set; }
        bool UseNotes { get; set; }

        // Weights (0-10)
        int WeightGender { get; set; }
        int WeightAddress { get; set; }
        int WeightJobTitle { get; set; }
        int WeightMaritalStatus { get; set; }
        int WeightProofNumber { get; set; }
        int WeightDbPrice { get; set; }
        int WeightHeadquarters { get; set; }
        int WeightInsuranceName { get; set; }
        int WeightCarJoinDate { get; set; }
        int WeightNotes { get; set; }

        event Action? Changed;
    }
}
