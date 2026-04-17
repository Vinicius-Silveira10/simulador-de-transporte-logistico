using System.ComponentModel.DataAnnotations;

namespace LogisticsTrackingAPI.Models
{
    public class Trip
    {
        [Key]
        public int Id { get; set; }
        
        public int PlayerId { get; set; }
        public Player? Player { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Origin { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string Destination { get; set; } = string.Empty;
        
        public decimal Revenue { get; set; } // O preço do frete
        
        public decimal TaxesAmount { get; set; } // 12% de ICMS
        
        public decimal KmCosts { get; set; } // Custo derivado da KM
        
        public decimal NetProfit { get; set; } // Receita - Impostos - CustoKm
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Negotiating"; // "Negotiating", "On Trip", "Finished"

        public string? IncidentLogs { get; set; } // Feed de Ocorrências da Viagem

        [MaxLength(200)]
        public string? ContractorNPC { get; set; } // Tipo do Contrato VIP (Alimentos, Peças, Medicamentos)
    }
}
