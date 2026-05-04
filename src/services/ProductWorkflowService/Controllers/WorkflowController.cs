using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductWorkflowService.DTOs;
using ProductWorkflowService.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProductWorkflowService.Controllers
{
    [ApiController]
    [Route("api/workflow/products")]
    [Authorize]
    public class WorkflowController : ControllerBase
    {
        private readonly WorkflowManager _workflowManager;

        public WorkflowController(WorkflowManager workflowManager) => _workflowManager = workflowManager;

        [HttpPut("{id}/pricing")]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<IActionResult> UpdatePricing(int id, PricingDto dto)
        {
            try { return Ok(await _workflowManager.SavePricingAsync(id, dto)); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPut("{id}/inventory")]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<IActionResult> UpdateInventory(int id, InventoryDto dto) =>
            Ok(await _workflowManager.SaveInventoryAsync(id, dto));

        [HttpPost("{id}/submit")]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<IActionResult> Submit(int id)
        {
            var user = User.FindFirstValue(ClaimTypes.Email) ?? "unknown";
            return Ok(await _workflowManager.SubmitForReviewAsync(id, user));
        }

        [HttpPost("{id}/enrich")]
        [Authorize(Roles = "Admin,ProductManager,ContentExecutive")]
        public async Task<IActionResult> MarkInEnrichment(int id)
        {
            try
            {
                var user = User.FindFirstValue(ClaimTypes.Email) ?? "unknown";
                return Ok(await _workflowManager.MarkInEnrichmentAsync(id, user));
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")] // TC12 Security constraint
        public async Task<IActionResult> UpdateStatus(int id, StatusUpdateDto dto)
        {
            if (!dto.IsValidStatus())
                return BadRequest(new { message = $"Invalid status value '{dto.Status}'. Allowed: Draft, In Enrichment, Ready for Review, Approved, Rejected, Published, Archived." });

            try
            {
                var admin = User.FindFirstValue(ClaimTypes.Email) ?? "admin";
                return Ok(await _workflowManager.UpdateStatusAsync(id, dto, admin));
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}
