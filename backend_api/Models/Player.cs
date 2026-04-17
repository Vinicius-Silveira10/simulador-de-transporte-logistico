using System.ComponentModel.DataAnnotations;

namespace LogisticsTrackingAPI.Models
{
    public class Player
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        // Fase 6 (Arquitetura Mestre-Escravo)
        [MaxLength(20)]
        public string AccessKey { get; set; } = string.Empty;
        
        public decimal NetWorth { get; set; }
        
        public int FleetSize { get; set; } = 1;

        public int CurrentDay { get; set; } = 1;
        public int MaxDays { get; set; } = 120; // NOVO: Duração customizada da temporada Feita Via Web
        
        // Fase 5 Expansão (Tycoon Options)
        public bool HasBankLoan { get; set; } = false;
        public decimal LoanDebt { get; set; } = 0m;
        public bool HasPremiumTires { get; set; } = false;
        public bool HasAdvancedGPS { get; set; } = false;
        
        // Relacionamento com as Viagens (Trips)
        public List<Trip> Trips { get; set; } = new();
    }
}
