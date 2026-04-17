using LogisticsTrackingAPI.Data;
using LogisticsTrackingAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace LogisticsTrackingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CompanyController(AppDbContext context)
        {
            _context = context;
        }

        // --- SISTEMA COMPETITIVO (LOGIN VIA TOKEN ESCOLHIDO PELA WEB) ---
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginPayload payload)
        {
            if (string.IsNullOrWhiteSpace(payload.Key)) return BadRequest("Token de acesso limpo.");
            
            // O Unity é apenas um Client Burro agora. Ele precisa injetar a chave secreta criada no React.
            var company = _context.Players.FirstOrDefault(p => p.AccessKey == payload.Key.ToUpper());
            
            if (company == null) {
                return Unauthorized(new { message = "Chave Inválida. Crie a Matriz no Site Web!"});
            }

            return Ok(new { id = company.Id, name = company.Name, currentDay = company.CurrentDay, maxDays = company.MaxDays, accessKey = company.AccessKey });
        }

        // --- SISTEMA MESTRE-ESCRAVO (CADASTRO WEB) ---
        [HttpPost("register/web")]
        public async Task<IActionResult> RegisterWeb([FromBody] RegisterPayload payload)
        {
            if (string.IsNullOrWhiteSpace(payload.Name)) return BadRequest();
            if (payload.MaxDays <= 0) payload.MaxDays = 120; // Safety lock do backend
            
            var company = _context.Players.FirstOrDefault(p => p.Name.ToLower() == payload.Name.ToLower());
            if (company != null) return BadRequest("Nome já registrado na bolsa.");

            var newKey = Guid.NewGuid().ToString().Substring(0, 6).ToUpper(); // Exemplo: A4F9D2
            
            company = new Player { Name = payload.Name, NetWorth = 5000m, FleetSize = 1, CurrentDay = 1, MaxDays = payload.MaxDays, AccessKey = newKey };
            _context.Players.Add(company);
            await _context.SaveChangesAsync();

            return Ok(new { id = company.Id, name = company.Name, accessKey = company.AccessKey });
        }

        // --- SISTEMA DE INTELIGÊNCIA DA JÚLIA ---
        [HttpGet("{id}/state")]
        public async Task<IActionResult> GetCompanyState(int id)
        {
            var company = await _context.Players.FindAsync(id);
            if (company == null) return NotFound("Empresa fantasma na rota.");

            return Ok(new { 
                currentDay = company.CurrentDay, 
                maxDays = company.MaxDays,
                fleetSize = company.FleetSize, 
                netWorth = company.NetWorth,
                hasBankLoan = company.HasBankLoan,
                hasPremiumTires = company.HasPremiumTires,
                hasAdvancedGPS = company.HasAdvancedGPS,
                loanDebt = company.LoanDebt
            });
        }

        // --- SISTEMA DE TEMPO E CONTAS (TYCOON) ---
        // É chamado pelo Unity a cada X segundos
        [HttpPost("{id}/tick-day")]
        public async Task<IActionResult> TickDay(int id)
        {
            var company = await _context.Players.FindAsync(id);
            if (company == null) return NotFound();

            company.CurrentDay += 1;
            await _context.SaveChangesAsync();
            
            return Ok(new { currentDay = company.CurrentDay });
        }

        [HttpPost("{id}/pay-bills")]
        public async Task<IActionResult> PayBills(int id)
        {
            var company = await _context.Players.FindAsync(id);
            if (company == null) return NotFound();

            // Matemática da Sobrevivência: 
            // Custo Mensal Base = R$ 1.500 Fixo + R$ 800 por caminhão na frota
            decimal monthlyBills = 1500m + (company.FleetSize * 800m);
            
            // Juros do Banco Tycoon
            if (company.HasBankLoan && company.LoanDebt > 0) {
                // Parcela Pesada de 5000
                decimal parcela = 5000m;
                monthlyBills += parcela;
                company.LoanDebt -= parcela;
                if (company.LoanDebt <= 0) company.HasBankLoan = false; // Quitada!
            }
            
            company.NetWorth -= monthlyBills; // Abate do saldo Global
            
            await _context.SaveChangesAsync();
            return Ok(new { paidAmount = monthlyBills, newBalance = company.NetWorth, loanDebt = company.LoanDebt });
        }

        // --- LOJA TYCOON EXPANSION (BANCO E PEÇAS) ---
        [HttpPost("{id}/take-loan")]
        public async Task<IActionResult> TakeLoan(int id)
        {
            var company = await _context.Players.FindAsync(id);
            if (company == null) return NotFound();

            if (company.HasBankLoan) return BadRequest("O Sr. Carvalho negou: Você já possui um empréstimo ativo!");

            company.HasBankLoan = true;
            company.LoanDebt += 30000m;    // Capital + Juros da simulação
            company.NetWorth += 30000m;    // Injeta na conta C-Level

            await _context.SaveChangesAsync();
            return Ok(new { newBalance = company.NetWorth, message = "Empréstimo Aprovado. Parcela cobrada nos dias 30."});
        }

        [HttpPost("{id}/buy-upgrade")]
        public async Task<IActionResult> BuyUpgrade(int id, [FromBody] UpgradePayload payload)
        {
            var company = await _context.Players.FindAsync(id);
            if (company == null) return NotFound();

            if (payload.Type == "tires") {
                if (company.HasPremiumTires) return BadRequest("Já possui Pneus Michelin em toda Frota!");
                if (company.NetWorth < 4000m) return BadRequest("Saldo Dívida p/ Peças Mestre.");
                company.NetWorth -= 4000m;
                company.HasPremiumTires = true;
            } else if (payload.Type == "gps") {
                if (company.HasAdvancedGPS) return BadRequest("Satélite Geo já ativo!");
                if (company.NetWorth < 2000m) return BadRequest("Saldo Dívida p/ GPS.");
                company.NetWorth -= 2000m;
                company.HasAdvancedGPS = true;
            } else {
                return BadRequest("Peça desconhecida.");
            }

            await _context.SaveChangesAsync();
            return Ok(new { newBalance = company.NetWorth, hasPremiumTires = company.HasPremiumTires, hasAdvancedGPS = company.HasAdvancedGPS });
        }

        // Mercador de Caminhões no Jogo (A Concessionária NPC)
        [HttpPost("{id}/buy-truck")]
        public async Task<IActionResult> BuyTruck(int id)
        {
            var company = await _context.Players.FindAsync(id);
            if (company == null) return NotFound();

            if (company.FleetSize >= 7) return BadRequest("Limite máximo atingido. A Garagem só comporta 7 caminhões!");

            decimal truckPrice = 12000m; // R$ 12.000 para um caminhão novo simulado
            if (company.NetWorth < truckPrice) return BadRequest("Saldo insuficiente no Cofre do Gerente.");

            company.NetWorth -= truckPrice;
            company.FleetSize += 1;

            await _context.SaveChangesAsync();
            return Ok(new { newFleetSize = company.FleetSize, newBalance = company.NetWorth, message = "Novo modelo logístico adquirido com Sucesso!" });
        }
    }

    public class UpgradePayload { public string Type { get; set; } }

    public class LoginPayload { public string Key { get; set; } }
    public class RegisterPayload { public string Name { get; set; } public int MaxDays { get; set; } }
}
