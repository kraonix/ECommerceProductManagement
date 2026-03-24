using AdminReportingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AdminReportingService.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly ReportingManager _reporting;
        public ReportsController(ReportingManager reporting) => _reporting = reporting;

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard() => Ok(await _reporting.GetDashboardSummaryAsync());

        [HttpGet("export")]
        public async Task<IActionResult> ExportDashboard() // TC13
        {
            var fileBytes = await _reporting.ExportDashboardDataAsync();
            return File(fileBytes, "text/csv", "dashboard_export.csv");
        }
    }
}
