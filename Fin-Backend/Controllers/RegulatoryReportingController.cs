using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.RegulatoryReporting;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Domain.Entities.RegulatoryReporting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.IO;
using AutoMapper;
using System.Linq;

namespace FinTech.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RegulatoryReportingController : ControllerBase
    {
        private readonly IRegulatoryReportingService _regulatoryReportingService;
        private readonly IMapper _mapper;
        
        public RegulatoryReportingController(
            IRegulatoryReportingService regulatoryReportingService,
            IMapper mapper)
        {
            _regulatoryReportingService = regulatoryReportingService ?? throw new ArgumentNullException(nameof(regulatoryReportingService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        
        #region Template Management
        
        [HttpGet("templates")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<RegulatoryReportTemplateDto>>> GetAllTemplates()
        {
            var templates = await _regulatoryReportingService.GetAllTemplatesAsync();
            return Ok(_mapper.Map<IEnumerable<RegulatoryReportTemplateDto>>(templates));
        }
        
        [HttpGet("templates/regulatory-body/{regulatoryBody}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<RegulatoryReportTemplateDto>>> GetTemplatesByRegulatoryBody(RegulatoryBody regulatoryBody)
        {
            var templates = await _regulatoryReportingService.GetTemplatesByRegulatoryBodyAsync(regulatoryBody);
            return Ok(_mapper.Map<IEnumerable<RegulatoryReportTemplateDto>>(templates));
        }
        
        [HttpGet("templates/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RegulatoryReportTemplateDto>> GetTemplateById(Guid id)
        {
            var template = await _regulatoryReportingService.GetTemplateByIdAsync(id);
            if (template == null)
            {
                return NotFound();
            }
            
            return Ok(_mapper.Map<RegulatoryReportTemplateDto>(template));
        }
        
        [HttpPost("templates")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<RegulatoryReportTemplateDto>> CreateTemplate(CreateRegulatoryReportTemplateDto createTemplateDto)
        {
            var template = _mapper.Map<RegulatoryReportTemplate>(createTemplateDto);
            var result = await _regulatoryReportingService.CreateTemplateAsync(template);
            
            var resultDto = _mapper.Map<RegulatoryReportTemplateDto>(result);
            return CreatedAtAction(nameof(GetTemplateById), new { id = result.Id }, resultDto);
        }
        
        [HttpPut("templates/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<RegulatoryReportTemplateDto>> UpdateTemplate(Guid id, UpdateRegulatoryReportTemplateDto updateTemplateDto)
        {
            if (id != updateTemplateDto.Id)
            {
                return BadRequest("ID mismatch between URL and request body");
            }
            
            var existingTemplate = await _regulatoryReportingService.GetTemplateByIdAsync(id);
            if (existingTemplate == null)
            {
                return NotFound();
            }
            
            var template = _mapper.Map(updateTemplateDto, existingTemplate);
            var result = await _regulatoryReportingService.UpdateTemplateAsync(template);
            
            return Ok(_mapper.Map<RegulatoryReportTemplateDto>(result));
        }
        
        [HttpDelete("templates/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteTemplate(Guid id)
        {
            var existingTemplate = await _regulatoryReportingService.GetTemplateByIdAsync(id);
            if (existingTemplate == null)
            {
                return NotFound();
            }
            
            await _regulatoryReportingService.DeleteTemplateAsync(id);
            return NoContent();
        }
        
        #endregion
        
        #region Section Management
        
        [HttpGet("templates/{templateId}/sections")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<RegulatoryReportSectionDto>>> GetSectionsByTemplateId(Guid templateId)
        {
            var template = await _regulatoryReportingService.GetTemplateByIdAsync(templateId);
            if (template == null)
            {
                return NotFound("Template not found");
            }
            
            var sections = await _regulatoryReportingService.GetSectionsByTemplateIdAsync(templateId);
            return Ok(_mapper.Map<IEnumerable<RegulatoryReportSectionDto>>(sections));
        }
        
        [HttpPost("sections")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<RegulatoryReportSectionDto>> CreateSection(CreateRegulatoryReportSectionDto createSectionDto)
        {
            var template = await _regulatoryReportingService.GetTemplateByIdAsync(createSectionDto.ReportTemplateId);
            if (template == null)
            {
                return BadRequest("Template not found");
            }
            
            var section = _mapper.Map<RegulatoryReportSection>(createSectionDto);
            var result = await _regulatoryReportingService.CreateSectionAsync(section);
            
            var resultDto = _mapper.Map<RegulatoryReportSectionDto>(result);
            return CreatedAtAction(nameof(GetSectionsByTemplateId), new { templateId = result.ReportTemplateId }, resultDto);
        }
        
        [HttpPut("sections/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<RegulatoryReportSectionDto>> UpdateSection(Guid id, UpdateRegulatoryReportSectionDto updateSectionDto)
        {
            if (id != updateSectionDto.Id)
            {
                return BadRequest("ID mismatch between URL and request body");
            }
            
            // First get the existing section
            var sections = await _regulatoryReportingService.GetSectionsByTemplateIdAsync(updateSectionDto.ReportTemplateId);
            var existingSection = sections.FirstOrDefault(s => s.Id == id);
            
            if (existingSection == null)
            {
                return NotFound();
            }
            
            var section = _mapper.Map(updateSectionDto, existingSection);
            var result = await _regulatoryReportingService.UpdateSectionAsync(section);
            
            return Ok(_mapper.Map<RegulatoryReportSectionDto>(result));
        }
        
        [HttpDelete("sections/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteSection(Guid id)
        {
            // We'll need to find the section first to confirm it exists
            // This is a simplification - in reality you'd need to query the repository
            var deleted = await _regulatoryReportingService.DeleteSectionAsync(id);
            
            if (!deleted)
            {
                return NotFound();
            }
            
            return NoContent();
        }
        
        #endregion
        
        #region Field Management
        
        [HttpGet("sections/{sectionId}/fields")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<RegulatoryReportFieldDto>>> GetFieldsBySectionId(Guid sectionId)
        {
            var fields = await _regulatoryReportingService.GetFieldsBySectionIdAsync(sectionId);
            return Ok(_mapper.Map<IEnumerable<RegulatoryReportFieldDto>>(fields));
        }
        
        [HttpPost("fields")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<RegulatoryReportFieldDto>> CreateField(CreateRegulatoryReportFieldDto createFieldDto)
        {
            var field = _mapper.Map<RegulatoryReportField>(createFieldDto);
            var result = await _regulatoryReportingService.CreateFieldAsync(field);
            
            var resultDto = _mapper.Map<RegulatoryReportFieldDto>(result);
            return CreatedAtAction(nameof(GetFieldsBySectionId), new { sectionId = result.SectionId }, resultDto);
        }
        
        [HttpPut("fields/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<RegulatoryReportFieldDto>> UpdateField(Guid id, UpdateRegulatoryReportFieldDto updateFieldDto)
        {
            if (id != updateFieldDto.Id)
            {
                return BadRequest("ID mismatch between URL and request body");
            }
            
            // We'll need to find the field first to map to
            var fields = await _regulatoryReportingService.GetFieldsBySectionIdAsync(updateFieldDto.SectionId);
            var existingField = fields.FirstOrDefault(f => f.Id == id);
            
            if (existingField == null)
            {
                return NotFound();
            }
            
            var field = _mapper.Map(updateFieldDto, existingField);
            var result = await _regulatoryReportingService.UpdateFieldAsync(field);
            
            return Ok(_mapper.Map<RegulatoryReportFieldDto>(result));
        }
        
        [HttpDelete("fields/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteField(Guid id)
        {
            var deleted = await _regulatoryReportingService.DeleteFieldAsync(id);
            
            if (!deleted)
            {
                return NotFound();
            }
            
            return NoContent();
        }
        
        #endregion
        
        #region Report Generation
        
        [HttpPost("submissions")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RegulatoryReportSubmissionDto>> InitiateSubmission(InitiateReportSubmissionDto initiateDto)
        {
            var template = await _regulatoryReportingService.GetTemplateByIdAsync(initiateDto.TemplateId);
            if (template == null)
            {
                return BadRequest("Template not found");
            }
            
            var submission = await _regulatoryReportingService.InitiateReportSubmissionAsync(
                initiateDto.TemplateId, 
                initiateDto.ReportingPeriodStart, 
                initiateDto.ReportingPeriodEnd);
            
            var resultDto = _mapper.Map<RegulatoryReportSubmissionDto>(submission);
            return CreatedAtAction(nameof(GetSubmissionById), new { id = submission.Id }, resultDto);
        }
        
        [HttpGet("submissions/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RegulatoryReportSubmissionDto>> GetSubmissionById(Guid id)
        {
            var submission = await _regulatoryReportingService.GetSubmissionByIdAsync(id);
            if (submission == null)
            {
                return NotFound();
            }
            
            return Ok(_mapper.Map<RegulatoryReportSubmissionDto>(submission));
        }
        
        [HttpGet("submissions/template/{templateId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<RegulatoryReportSubmissionDto>>> GetSubmissionsByTemplateId(
            Guid templateId, 
            [FromQuery] DateTime? fromDate, 
            [FromQuery] DateTime? toDate)
        {
            var submissions = await _regulatoryReportingService.GetSubmissionsByTemplateIdAsync(templateId, fromDate, toDate);
            return Ok(_mapper.Map<IEnumerable<RegulatoryReportSubmissionDto>>(submissions));
        }
        
        [HttpGet("submissions/pending")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<IEnumerable<RegulatoryReportSubmissionDto>>> GetPendingSubmissions()
        {
            var submissions = await _regulatoryReportingService.GetPendingSubmissionsAsync();
            return Ok(_mapper.Map<IEnumerable<RegulatoryReportSubmissionDto>>(submissions));
        }
        
        [HttpPost("submissions/{id}/populate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RegulatoryReportSubmissionDto>> PopulateSubmission(Guid id)
        {
            var submission = await _regulatoryReportingService.GetSubmissionByIdAsync(id);
            if (submission == null)
            {
                return NotFound();
            }
            
            var result = await _regulatoryReportingService.PopulateReportDataAsync(id);
            return Ok(_mapper.Map<RegulatoryReportSubmissionDto>(result));
        }
        
        [HttpPost("submissions/{id}/validate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<RegulatoryReportValidationDto>>> ValidateSubmission(Guid id)
        {
            var submission = await _regulatoryReportingService.GetSubmissionByIdAsync(id);
            if (submission == null)
            {
                return NotFound();
            }
            
            var validations = await _regulatoryReportingService.ValidateReportAsync(id);
            return Ok(_mapper.Map<IEnumerable<RegulatoryReportValidationDto>>(validations));
        }
        
        [HttpPost("submissions/{id}/generate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> GenerateReportFile(Guid id)
        {
            var submission = await _regulatoryReportingService.GetSubmissionByIdAsync(id);
            if (submission == null)
            {
                return NotFound();
            }
            
            var filePath = await _regulatoryReportingService.GenerateReportFileAsync(id);
            return Ok(new { FilePath = filePath });
        }
        
        [HttpPost("submissions/{id}/submit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RegulatoryReportSubmissionDto>> SubmitReport(Guid id)
        {
            var submission = await _regulatoryReportingService.GetSubmissionByIdAsync(id);
            if (submission == null)
            {
                return NotFound();
            }
            
            try
            {
                var result = await _regulatoryReportingService.SubmitReportAsync(id);
                return Ok(_mapper.Map<RegulatoryReportSubmissionDto>(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPost("submissions/{id}/approve")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<RegulatoryReportSubmissionDto>> ApproveSubmission(Guid id, [FromBody] ApproveReportDto approveDto)
        {
            var submission = await _regulatoryReportingService.GetSubmissionByIdAsync(id);
            if (submission == null)
            {
                return NotFound();
            }
            
            try
            {
                var result = await _regulatoryReportingService.ApproveSubmissionAsync(id, approveDto.ApproverId, approveDto.Comments);
                return Ok(_mapper.Map<RegulatoryReportSubmissionDto>(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPost("submissions/{id}/reject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<RegulatoryReportSubmissionDto>> RejectSubmission(Guid id, [FromBody] RejectReportDto rejectDto)
        {
            var submission = await _regulatoryReportingService.GetSubmissionByIdAsync(id);
            if (submission == null)
            {
                return NotFound();
            }
            
            try
            {
                var result = await _regulatoryReportingService.RejectSubmissionAsync(id, rejectDto.Reason);
                return Ok(_mapper.Map<RegulatoryReportSubmissionDto>(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        #endregion
        
        #region Schedule Management
        
        [HttpPost("schedules")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<RegulatoryReportScheduleDto>> CreateSchedule(CreateRegulatoryReportScheduleDto createScheduleDto)
        {
            var template = await _regulatoryReportingService.GetTemplateByIdAsync(createScheduleDto.ReportTemplateId);
            if (template == null)
            {
                return BadRequest("Template not found");
            }
            
            var schedule = _mapper.Map<RegulatoryReportSchedule>(createScheduleDto);
            var result = await _regulatoryReportingService.CreateScheduleAsync(schedule);
            
            var resultDto = _mapper.Map<RegulatoryReportScheduleDto>(result);
            return CreatedAtAction(nameof(GetUpcomingSchedules), new { daysAhead = 30 }, resultDto);
        }
        
        [HttpPut("schedules/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<RegulatoryReportScheduleDto>> UpdateSchedule(Guid id, UpdateRegulatoryReportScheduleDto updateScheduleDto)
        {
            if (id != updateScheduleDto.Id)
            {
                return BadRequest("ID mismatch between URL and request body");
            }
            
            // In reality, you'd query the repository to get the existing schedule
            // This is a simplification
            
            var schedule = _mapper.Map<RegulatoryReportSchedule>(updateScheduleDto);
            var result = await _regulatoryReportingService.UpdateScheduleAsync(schedule);
            
            return Ok(_mapper.Map<RegulatoryReportScheduleDto>(result));
        }
        
        [HttpDelete("schedules/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteSchedule(Guid id)
        {
            var deleted = await _regulatoryReportingService.DeleteScheduleAsync(id);
            
            if (!deleted)
            {
                return NotFound();
            }
            
            return NoContent();
        }
        
        [HttpGet("schedules/upcoming/{daysAhead}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<RegulatoryReportScheduleDto>>> GetUpcomingSchedules(int daysAhead)
        {
            var schedules = await _regulatoryReportingService.GetUpcomingSchedulesAsync(daysAhead);
            return Ok(_mapper.Map<IEnumerable<RegulatoryReportScheduleDto>>(schedules));
        }
        
        #endregion
        
        #region Report Generation Shortcuts
        
        [HttpGet("generate/cbn/monthly/{year}/{month}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GenerateCBNMonthlyReturn(int year, int month)
        {
            try
            {
                if (month < 1 || month > 12)
                {
                    return BadRequest("Month must be between 1 and 12");
                }
                
                var reportingPeriod = new DateTime(year, month, 1);
                var fileContent = await _regulatoryReportingService.GenerateCBNMonthlyReturnAsync(reportingPeriod);
                
                return File(fileContent, "application/octet-stream", $"CBN_Monthly_Return_{year}_{month:D2}.xml");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpGet("generate/cbn/quarterly/{year}/{quarter}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GenerateCBNQuarterlyReturn(int year, int quarter)
        {
            try
            {
                if (quarter < 1 || quarter > 4)
                {
                    return BadRequest("Quarter must be between 1 and 4");
                }
                
                var month = (quarter - 1) * 3 + 1;
                var reportingPeriod = new DateTime(year, month, 1);
                var fileContent = await _regulatoryReportingService.GenerateCBNQuarterlyReturnAsync(reportingPeriod);
                
                return File(fileContent, "application/octet-stream", $"CBN_Quarterly_Return_{year}_Q{quarter}.xml");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpGet("generate/ndic/premium/{year}/{quarter}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GenerateNDICPremiumReturn(int year, int quarter)
        {
            try
            {
                if (quarter < 1 || quarter > 4)
                {
                    return BadRequest("Quarter must be between 1 and 4");
                }
                
                var month = (quarter - 1) * 3 + 1;
                var reportingPeriod = new DateTime(year, month, 1);
                var fileContent = await _regulatoryReportingService.GenerateNDICPremiumReturnAsync(reportingPeriod);
                
                return File(fileContent, "application/octet-stream", $"NDIC_Premium_Return_{year}_Q{quarter}.xml");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpGet("generate/ndic/deposit/{year}/{month}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GenerateNDICDepositReturns(int year, int month)
        {
            try
            {
                if (month < 1 || month > 12)
                {
                    return BadRequest("Month must be between 1 and 12");
                }
                
                var reportingPeriod = new DateTime(year, month, 1);
                var fileContent = await _regulatoryReportingService.GenerateNDICDepositReturnsAsync(reportingPeriod);
                
                return File(fileContent, "application/octet-stream", $"NDIC_Deposit_Returns_{year}_{month:D2}.xml");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpGet("generate/firs/monthly/{year}/{month}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GenerateFIRSMonthlyTaxReturn(int year, int month)
        {
            try
            {
                if (month < 1 || month > 12)
                {
                    return BadRequest("Month must be between 1 and 12");
                }
                
                var reportingPeriod = new DateTime(year, month, 1);
                var fileContent = await _regulatoryReportingService.GenerateFIRSMonthlyTaxReturnAsync(reportingPeriod);
                
                return File(fileContent, "application/octet-stream", $"FIRS_Monthly_Tax_Return_{year}_{month:D2}.xml");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpGet("generate/firs/annual/{year}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GenerateFIRSAnnualReturn(int year)
        {
            try
            {
                var fileContent = await _regulatoryReportingService.GenerateFIRSAnnualReturnAsync(year);
                
                return File(fileContent, "application/octet-stream", $"FIRS_Annual_Return_{year}.xml");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        #endregion
    }
}
