using System;
using NexaCRM.WebClient.Services.Interfaces;

namespace NexaCRM.WebClient.Services
{
    public class DedupeConfigService : IDedupeConfigService
    {
        private bool _enabled;
        private int _days = 30;
        private bool _includeFuzzy;
        private int _scoreThreshold = 50;
        private int _monitorIntervalMinutes = 10;
        private bool _notifyOnSameCount;

        // toggles
        public bool UseGender { get; set; }
        public bool UseAddress { get; set; }
        public bool UseJobTitle { get; set; }
        public bool UseMaritalStatus { get; set; }
        public bool UseProofNumber { get; set; }
        public bool UseDbPrice { get; set; }
        public bool UseHeadquarters { get; set; }
        public bool UseInsuranceName { get; set; }
        public bool UseCarJoinDate { get; set; }
        public bool UseNotes { get; set; }

        // weights
        public int WeightGender { get; set; } = 1;
        public int WeightAddress { get; set; } = 2;
        public int WeightJobTitle { get; set; } = 2;
        public int WeightMaritalStatus { get; set; } = 1;
        public int WeightProofNumber { get; set; } = 3;
        public int WeightDbPrice { get; set; } = 2;
        public int WeightHeadquarters { get; set; } = 1;
        public int WeightInsuranceName { get; set; } = 2;
        public int WeightCarJoinDate { get; set; } = 2;
        public int WeightNotes { get; set; } = 1;

        public bool Enabled { get => _enabled; set { _enabled = value; Changed?.Invoke(); } }
        public int Days { get => _days; set { _days = value; Changed?.Invoke(); } }
        public bool IncludeFuzzy { get => _includeFuzzy; set { _includeFuzzy = value; Changed?.Invoke(); } }
        public int ScoreThreshold { get => _scoreThreshold; set { _scoreThreshold = value; Changed?.Invoke(); } }
        public int MonitorIntervalMinutes { get => _monitorIntervalMinutes; set { _monitorIntervalMinutes = value; Changed?.Invoke(); } }
        public bool NotifyOnSameCount { get => _notifyOnSameCount; set { _notifyOnSameCount = value; Changed?.Invoke(); } }

        public event Action? Changed;
    }
}
