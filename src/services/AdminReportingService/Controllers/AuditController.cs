using AdminReportingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AdminReportingService.Controllers
{
    [ApiController]
    [Route("api/audit")]
    [Authorize(Roles = "Admin,ProductManager")]
    public class AuditController : ControllerBase
    {
        private readonly ReportingManager _reporting;
        public AuditController(ReportingManager reporting) => _reporting = reporting;

        [HttpGet("products/{id}")]
        public async Task<IActionResult> GetProductHistory(int id) =>
            Ok(await _reporting.GetProductAuditHistoryAsync(id)); // TC14
    }
}
