using AutoMapper;
using FinTech.Core.Application.DTOs.Common;
using FinTech.Core.Application.DTOs.RegulatoryReporting;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Domain.Entities.RegulatoryReporting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FinTech.Presentation.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RegulatoryReportingController : ControllerBase
    {
        private readonly IRegulatoryReportingService _reportingService;
        private readonly IMapper _mapper;

        public RegulatoryReportingController(IRegulatoryReportingService reportingService, IMapper mapper)
        {
            _reportingService = reportingService;
            _mapper = mapper;
        }

        #region Templates

        [HttpGet("templates")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<BaseResponse<IEnumerable<RegulatoryReportTemplateDto>>>> GetAllTemplates()
        {
            var templates = await _reportingService.GetAllTemplatesAsync();
            var templatesDto = _mapper.Map<IEnumerable<RegulatoryReportTemplateDto>>(templates);
            
            return Ok(new BaseResponse<IEnumerable<RegulatoryReportTemplateDto>>
            {
                Success = true,
                Message = "Templates retrieved successfully",
                Data = templatesDto
            });
        }

        [HttpGet("templates/regulatory-body/{regulatoryBody}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BaseResponse<IEnumerable<RegulatoryReportTemplateDto>>>> GetTemplatesByRegulatoryBody(RegulatoryBody regulatoryBody)
        {
            var templates = await _reportingService.GetTemplatesByRegulatoryBodyAsync(regulatoryBody);
            var templatesDto = _mapper.Map<IEnumerable<RegulatoryReportTemplateDto>>(templates);
            
            return Ok(new BaseResponse<IEnumerable<RegulatoryReportTemplateDto>>
            {
                Success = true,
                Message = $"Templates for {regulatoryBody} retrieved successfully",
                Data = templatesDto
            });
        }

        [HttpGet("templates/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BaseResponse<RegulatoryReportTemplateDetailDto>>> GetTemplateById(Guid id)
        {
            var template = await _reportingService.GetTemplateByIdAsync(id);
            if (template == null)
            {
                return NotFound(new BaseResponse<RegulatoryReportTemplateDetailDto>
                {
                    Success = false,
                    Message = $"Template with ID {id} not found"
                });
            }
            
            var templateDto = _mapper.Map<RegulatoryReportTemplateDetailDto>(template);
            
            return Ok(new BaseResponse<RegulatoryReportTemplateDetailDto>
            {
                Success = true,
                Message = "Template retrieved successfully",
                Data = templateDto
            });
        }

        [HttpPost("templates")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,ReportingManager")]
        public async Task<ActionResult<BaseResponse<RegulatoryReportTemplateDto>>> CreateTemplate([FromBody] CreateRegulatoryReportTemplateDto createDto)
        {
            try
            {
                var template = _mapper.Map<RegulatoryReportTemplate>(createDto);
                var createdTemplate = await _reportingService.CreateTemplateAsync(template);
                var templateDto = _mapper.Map<RegulatoryReportTemplateDto>(createdTemplate);
                
                return CreatedAtAction(nameof(GetTemplateById), new { id = templateDto.Id }, new BaseResponse<RegulatoryReportTemplateDto>
                {
                    Success = true,
                    Message = "Template created successfully",
                    Data = templateDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<RegulatoryReportTemplateDto>
                {
                    Success = false,
                    Message = $"Error creating template: {ex.Message}"
                });
            }
        }

        [HttpPut("templates/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin,ReportingManager")]
        public async Task<ActionResult<BaseResponse<RegulatoryReportTemplateDto>>> UpdateTemplate(Guid id, [FromBody] UpdateRegulatoryReportTemplateDto updateDto)
        {
            if (id != updateDto.Id)
            {
                return BadRequest(new BaseResponse<RegulatoryReportTemplateDto>
                {
                    Success = false,
                    Message = "ID mismatch between route and body"
                });
            }
            
            try
            {
                var existingTemplate = await _reportingService.GetTemplateByIdAsync(id);
                if (existingTemplate == null)
                {
                    return NotFound(new BaseResponse<RegulatoryReportTemplateDto>
                    {
                        Success = false,
                        Message = $"Template with ID {id} not found"
                    });
                }
                
                var template = _mapper.Map<RegulatoryReportTemplate>(updateDto);
                var updatedTemplate = await _reportingService.UpdateTemplateAsync(template);
                var templateDto = _mapper.Map<RegulatoryReportTemplateDto>(updatedTemplate);
                
                return Ok(new BaseResponse<RegulatoryReportTemplateDto>
                {
                    Success = true,
                    Message = "Template updated successfully",
                    Data = templateDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<RegulatoryReportTemplateDto>
                {
                    Success = false,
                    Message = $"Error updating template: {ex.Message}"
                });
            }
        }

        [HttpDelete("templates/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin,ReportingManager")]
        public async Task<ActionResult<BaseResponse<bool>>> DeleteTemplate(Guid id)
        {
            try
            {
                var existingTemplate = await _reportingService.GetTemplateByIdAsync(id);
                if (existingTemplate == null)
                {
                    return NotFound(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = $"Template with ID {id} not found"
                    });
                }
                
                var result = await _reportingService.DeleteTemplateAsync(id);
                
                return Ok(new BaseResponse<bool>
                {
                    Success = true,
                    Message = "Template deleted successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<bool>
                {
                    Success = false,
                    Message = $"Error deleting template: {ex.Message}"
                });
            }
        }

        #endregion

        #region Sections

        [HttpGet("templates/{templateId}/sections")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BaseResponse<IEnumerable<RegulatoryReportSectionDto>>>> GetSectionsByTemplateId(Guid templateId)
        {
            var template = await _reportingService.GetTemplateByIdAsync(templateId);
            if (template == null)
            {
                return NotFound(new BaseResponse<IEnumerable<RegulatoryReportSectionDto>>
                {
                    Success = false,
                    Message = $"Template with ID {templateId} not found"
                });
            }
            
            var sections = await _reportingService.GetSectionsByTemplateIdAsync(templateId);
            var sectionsDto = _mapper.Map<IEnumerable<RegulatoryReportSectionDto>>(sections);
            
            return Ok(new BaseResponse<IEnumerable<RegulatoryReportSectionDto>>
            {
                Success = true,
                Message = "Sections retrieved successfully",
                Data = sectionsDto
            });
        }

        [HttpPost("sections")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,ReportingManager")]
        public async Task<ActionResult<BaseResponse<RegulatoryReportSectionDto>>> CreateSection([FromBody] CreateRegulatoryReportSectionDto createDto)
        {
            try
            {
                var section = _mapper.Map<RegulatoryReportSection>(createDto);
                var createdSection = await _reportingService.CreateSectionAsync(section);
                var sectionDto = _mapper.Map<RegulatoryReportSectionDto>(createdSection);
                
                return CreatedAtAction(nameof(GetSectionsByTemplateId), new { templateId = sectionDto.ReportTemplateId }, new BaseResponse<RegulatoryReportSectionDto>
                {
                    Success = true,
                    Message = "Section created successfully",
                    Data = sectionDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<RegulatoryReportSectionDto>
                {
                    Success = false,
                    Message = $"Error creating section: {ex.Message}"
                });
            }
        }

        [HttpPut("sections/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin,ReportingManager")]
        public async Task<ActionResult<BaseResponse<RegulatoryReportSectionDto>>> UpdateSection(Guid id, [FromBody] UpdateRegulatoryReportSectionDto updateDto)
        {
            if (id != updateDto.Id)
            {
                return BadRequest(new BaseResponse<RegulatoryReportSectionDto>
                {
                    Success = false,
                    Message = "ID mismatch between route and body"
                });
            }
            
            try
            {
                var section = _mapper.Map<RegulatoryReportSection>(updateDto);
                var updatedSection = await _reportingService.UpdateSectionAsync(section);
                var sectionDto = _mapper.Map<RegulatoryReportSectionDto>(updatedSection);
                
                return Ok(new BaseResponse<RegulatoryReportSectionDto>
                {
                    Success = true,
                    Message = "Section updated successfully",
                    Data = sectionDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<RegulatoryReportSectionDto>
                {
                    Success = false,
                    Message = $"Error updating section: {ex.Message}"
                });
            }
        }

        [HttpDelete("sections/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin,ReportingManager")]
        public async Task<ActionResult<BaseResponse<bool>>> DeleteSection(Guid id)
        {
            try
            {
                var result = await _reportingService.DeleteSectionAsync(id);
                
                return Ok(new BaseResponse<bool>
                {
                    Success = true,
                    Message = "Section deleted successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<bool>
                {
                    Success = false,
                    Message = $"Error deleting section: {ex.Message}"
                });
            }
        }

        #endregion

        #region Fields

        [HttpGet("sections/{sectionId}/fields")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<BaseResponse<IEnumerable<RegulatoryReportFieldDto>>>> GetFieldsBySectionId(Guid sectionId)
        {
            var fields = await _reportingService.GetFieldsBySectionIdAsync(sectionId);
            var fieldsDto = _mapper.Map<IEnumerable<RegulatoryReportFieldDto>>(fields);
            
            return Ok(new BaseResponse<IEnumerable<RegulatoryReportFieldDto>>
            {
                Success = true,
                Message = "Fields retrieved successfully",
                Data = fieldsDto
            });
        }

        [HttpPost("fields")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,ReportingManager")]
        public async Task<ActionResult<BaseResponse<RegulatoryReportFieldDto>>> CreateField([FromBody] CreateRegulatoryReportFieldDto createDto)
        {
            try
            {
                var field = _mapper.Map<RegulatoryReportField>(createDto);
                var createdField = await _reportingService.CreateFieldAsync(field);
                var fieldDto = _mapper.Map<RegulatoryReportFieldDto>(createdField);
                
                return CreatedAtAction(nameof(GetFieldsBySectionId), new { sectionId = fieldDto.SectionId }, new BaseResponse<RegulatoryReportFieldDto>
                {
                    Success = true,
                    Message = "Field created successfully",
                    Data = fieldDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<RegulatoryReportFieldDto>
                {
                    Success = false,
                    Message = $"Error creating field: {ex.Message}"
                });
            }
        }

        [HttpPut("fields/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin,ReportingManager")]
        public async Task<ActionResult<BaseResponse<RegulatoryReportFieldDto>>> UpdateField(Guid id, [FromBody] UpdateRegulatoryReportFieldDto updateDto)
        {
            if (id != updateDto.Id)
            {
                return BadRequest(new BaseResponse<RegulatoryReportFieldDto>
                {
                    Success = false,
                    Message = "ID mismatch between route and body"
                });
            }
            
            try
            {
                var field = _mapper.Map<RegulatoryReportField>(updateDto);
                var updatedField = await _reportingService.UpdateFieldAsync(field);
                var fieldDto = _mapper.Map<RegulatoryReportFieldDto>(updatedField);
                
                return Ok(new BaseResponse<RegulatoryReportFieldDto>
                {
                    Success = true,
                    Message = "Field updated successfully",
                    Data = fieldDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<RegulatoryReportFieldDto>
                {
                    Success = false,
                    Message = $"Error updating field: {ex.Message}"
                });
            }
        }

        [HttpDelete("fields/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin,ReportingManager")]
        public async Task<ActionResult<BaseResponse<bool>>> DeleteField(Guid id)
        {
            try
            {
                var result = await _reportingService.DeleteFieldAsync(id);
                
                return Ok(new BaseResponse<bool>
                {
                    Success = true,
                    Message = "Field deleted successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<bool>
                {
                    Success = false,
                    Message = $"Error deleting field: {ex.Message}"
                });
            }
        }

        #endregion

        #region Report Submissions

        [HttpGet("submissions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<BaseResponse<IEnumerable<RegulatoryReportSubmissionDto>>>> GetPendingSubmissions()
        {
            var submissions = await _reportingService.GetPendingSubmissionsAsync();
            var submissionsDto = _mapper.Map<IEnumerable<RegulatoryReportSubmissionDto>>(submissions);
            
            return Ok(new BaseResponse<IEnumerable<RegulatoryReportSubmissionDto>>
            {
                Success = true,
                Message = "Pending submissions retrieved successfully",
                Data = submissionsDto
            });
        }

        [HttpGet("submissions/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BaseResponse<RegulatoryReportSubmissionDetailDto>>> GetSubmissionById(Guid id)
        {
            var submission = await _reportingService.GetSubmissionByIdAsync(id);
            if (submission == null)
            {
                return NotFound(new BaseResponse<RegulatoryReportSubmissionDetailDto>
                {
                    Success = false,
                    Message = $"Submission with ID {id} not found"
                });
            }
            
            var submissionDto = _mapper.Map<RegulatoryReportSubmissionDetailDto>(submission);
            
            return Ok(new BaseResponse<RegulatoryReportSubmissionDetailDto>
            {
                Success = true,
                Message = "Submission retrieved successfully",
                Data = submissionDto
            });
        }

        [HttpGet("templates/{templateId}/submissions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BaseResponse<IEnumerable<RegulatoryReportSubmissionDto>>>> GetSubmissionsByTemplateId(
            Guid templateId, 
            [FromQuery] DateTime? fromDate = null, 
            [FromQuery] DateTime? toDate = null)
        {
            var template = await _reportingService.GetTemplateByIdAsync(templateId);
            if (template == null)
            {
                return NotFound(new BaseResponse<IEnumerable<RegulatoryReportSubmissionDto>>
                {
                    Success = false,
                    Message = $"Template with ID {templateId} not found"
                });
            }
            
            var submissions = await _reportingService.GetSubmissionsByTemplateIdAsync(templateId, fromDate, toDate);
            var submissionsDto = _mapper.Map<IEnumerable<RegulatoryReportSubmissionDto>>(submissions);
            
            return Ok(new BaseResponse<IEnumerable<RegulatoryReportSubmissionDto>>
            {
                Success = true,
                Message = "Submissions retrieved successfully",
                Data = submissionsDto
            });
        }

        [HttpPost("submissions")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,ReportingManager,ReportingOfficer")]
        public async Task<ActionResult<BaseResponse<RegulatoryReportSubmissionDto>>> InitiateSubmission([FromBody] CreateRegulatoryReportSubmissionDto createDto)
        {
            try
            {
                var submission = await _reportingService.InitiateReportSubmissionAsync(
                    createDto.ReportTemplateId, 
                    createDto.ReportingPeriodStart, 
                    createDto.ReportingPeriodEnd);
                
                var submissionDto = _mapper.Map<RegulatoryReportSubmissionDto>(submission);
                
                return CreatedAtAction(nameof(GetSubmissionById), new { id = submissionDto.Id }, new BaseResponse<RegulatoryReportSubmissionDto>
                {
                    Success = true,
                    Message = "Submission initiated successfully",
                    Data = submissionDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<RegulatoryReportSubmissionDto>
                {
                    Success = false,
                    Message = $"Error initiating submission: {ex.Message}"
                });
            }
        }

        [HttpPost("submissions/{id}/populate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin,ReportingManager,ReportingOfficer")]
        public async Task<ActionResult<BaseResponse<RegulatoryReportSubmissionDto>>> PopulateSubmission(Guid id)
        {
            try
            {
                var submission = await _reportingService.GetSubmissionByIdAsync(id);
                if (submission == null)
                {
                    return NotFound(new BaseResponse<RegulatoryReportSubmissionDto>
                    {
                        Success = false,
                        Message = $"Submission with ID {id} not found"
                    });
                }
                
                var populatedSubmission = await _reportingService.PopulateReportDataAsync(id);
                var submissionDto = _mapper.Map<RegulatoryReportSubmissionDto>(populatedSubmission);
                
                return Ok(new BaseResponse<RegulatoryReportSubmissionDto>
                {
                    Success = true,
                    Message = "Submission data populated successfully",
                    Data = submissionDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<RegulatoryReportSubmissionDto>
                {
                    Success = false,
                    Message = $"Error populating submission data: {ex.Message}"
                });
            }
        }

        [HttpPost("submissions/{id}/validate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin,ReportingManager,ReportingOfficer")]
        public async Task<ActionResult<BaseResponse<IEnumerable<RegulatoryReportValidationDto>>>> ValidateSubmission(Guid id)
        {
            try
            {
                var submission = await _reportingService.GetSubmissionByIdAsync(id);
                if (submission == null)
                {
                    return NotFound(new BaseResponse<IEnumerable<RegulatoryReportValidationDto>>
                    {
                        Success = false,
                        Message = $"Submission with ID {id} not found"
                    });
                }
                
                var validations = await _reportingService.ValidateReportAsync(id);
                var validationsDto = _mapper.Map<IEnumerable<RegulatoryReportValidationDto>>(validations);
                
                return Ok(new BaseResponse<IEnumerable<RegulatoryReportValidationDto>>
                {
                    Success = true,
                    Message = $"Submission validated with {validations.Count()} issues",
                    Data = validationsDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<IEnumerable<RegulatoryReportValidationDto>>
                {
                    Success = false,
                    Message = $"Error validating submission: {ex.Message}"
                });
            }
        }

        [HttpPost("submissions/{id}/generate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin,ReportingManager,ReportingOfficer")]
        public async Task<ActionResult<BaseResponse<string>>> GenerateReportFile(Guid id)
        {
            try
            {
                var submission = await _reportingService.GetSubmissionByIdAsync(id);
                if (submission == null)
                {
                    return NotFound(new BaseResponse<string>
                    {
                        Success = false,
                        Message = $"Submission with ID {id} not found"
                    });
                }
                
                var filePath = await _reportingService.GenerateReportFileAsync(id);
                
                return Ok(new BaseResponse<string>
                {
                    Success = true,
                    Message = "Report file generated successfully",
                    Data = filePath
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>
                {
                    Success = false,
                    Message = $"Error generating report file: {ex.Message}"
                });
            }
        }

        [HttpPost("submissions/{id}/submit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin,ReportingManager,ReportingOfficer")]
        public async Task<ActionResult<BaseResponse<RegulatoryReportSubmissionDto>>> SubmitReport(Guid id)
        {
            try
            {
                var submission = await _reportingService.GetSubmissionByIdAsync(id);
                if (submission == null)
                {
                    return NotFound(new BaseResponse<RegulatoryReportSubmissionDto>
                    {
                        Success = false,
                        Message = $"Submission with ID {id} not found"
                    });
                }
                
                var submittedSubmission = await _reportingService.SubmitReportAsync(id);
                var submissionDto = _mapper.Map<RegulatoryReportSubmissionDto>(submittedSubmission);
                
                return Ok(new BaseResponse<RegulatoryReportSubmissionDto>
                {
                    Success = true,
                    Message = "Report submitted successfully",
                    Data = submissionDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<RegulatoryReportSubmissionDto>
                {
                    Success = false,
                    Message = $"Error submitting report: {ex.Message}"
                });
            }
        }

        [HttpPost("submissions/{id}/approve")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin,ReportingManager")]
        public async Task<ActionResult<BaseResponse<RegulatoryReportSubmissionDto>>> ApproveSubmission(Guid id, [FromBody] ApproveRegulatoryReportSubmissionDto approveDto)
        {
            try
            {
                var submission = await _reportingService.GetSubmissionByIdAsync(id);
                if (submission == null)
                {
                    return NotFound(new BaseResponse<RegulatoryReportSubmissionDto>
                    {
                        Success = false,
                        Message = $"Submission with ID {id} not found"
                    });
                }
                
                var approvedSubmission = await _reportingService.ApproveSubmissionAsync(id, approveDto.ApprovedById, approveDto.Comments);
                var submissionDto = _mapper.Map<RegulatoryReportSubmissionDto>(approvedSubmission);
                
                return Ok(new BaseResponse<RegulatoryReportSubmissionDto>
                {
                    Success = true,
                    Message = "Submission approved successfully",
                    Data = submissionDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<RegulatoryReportSubmissionDto>
                {
                    Success = false,
                    Message = $"Error approving submission: {ex.Message}"
                });
            }
        }

        [HttpPost("submissions/{id}/reject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin,ReportingManager")]
        public async Task<ActionResult<BaseResponse<RegulatoryReportSubmissionDto>>> RejectSubmission(Guid id, [FromBody] string reason)
        {
            try
            {
                var submission = await _reportingService.GetSubmissionByIdAsync(id);
                if (submission == null)
                {
                    return NotFound(new BaseResponse<RegulatoryReportSubmissionDto>
                    {
                        Success = false,
                        Message = $"Submission with ID {id} not found"
                    });
                }
                
                var rejectedSubmission = await _reportingService.RejectSubmissionAsync(id, reason);
                var submissionDto = _mapper.Map<RegulatoryReportSubmissionDto>(rejectedSubmission);
                
                return Ok(new BaseResponse<RegulatoryReportSubmissionDto>
                {
                    Success = true,
                    Message = "Submission rejected successfully",
                    Data = submissionDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<RegulatoryReportSubmissionDto>
                {
                    Success = false,
                    Message = $"Error rejecting submission: {ex.Message}"
                });
            }
        }

        #endregion

        #region Regulatory Body Specific Reports

        [HttpGet("cbn/monthly/{year}/{month}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,ReportingManager,ReportingOfficer")]
        public async Task<ActionResult> GenerateCBNMonthlyReturn(int year, int month)
        {
            try
            {
                if (month < 1 || month > 12)
                {
                    return BadRequest(new BaseResponse<object>
                    {
                        Success = false,
                        Message = "Month must be between 1 and 12"
                    });
                }
                
                var reportingPeriod = new DateTime(year, month, 1);
                var fileBytes = await _reportingService.GenerateCBNMonthlyReturnAsync(reportingPeriod);
                
                return File(
                    fileBytes, 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    $"CBN_Monthly_Return_{year}_{month:D2}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<object>
                {
                    Success = false,
                    Message = $"Error generating CBN Monthly Return: {ex.Message}"
                });
            }
        }

        [HttpGet("cbn/quarterly/{year}/{quarter}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,ReportingManager,ReportingOfficer")]
        public async Task<ActionResult> GenerateCBNQuarterlyReturn(int year, int quarter)
        {
            try
            {
                if (quarter < 1 || quarter > 4)
                {
                    return BadRequest(new BaseResponse<object>
                    {
                        Success = false,
                        Message = "Quarter must be between 1 and 4"
                    });
                }
                
                var month = (quarter - 1) * 3 + 1;
                var reportingPeriod = new DateTime(year, month, 1);
                var fileBytes = await _reportingService.GenerateCBNQuarterlyReturnAsync(reportingPeriod);
                
                return File(
                    fileBytes, 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    $"CBN_Quarterly_Return_{year}_Q{quarter}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<object>
                {
                    Success = false,
                    Message = $"Error generating CBN Quarterly Return: {ex.Message}"
                });
            }
        }

        [HttpGet("ndic/premium/{year}/{quarter}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,ReportingManager,ReportingOfficer")]
        public async Task<ActionResult> GenerateNDICPremiumReturn(int year, int quarter)
        {
            try
            {
                if (quarter < 1 || quarter > 4)
                {
                    return BadRequest(new BaseResponse<object>
                    {
                        Success = false,
                        Message = "Quarter must be between 1 and 4"
                    });
                }
                
                var month = (quarter - 1) * 3 + 1;
                var reportingPeriod = new DateTime(year, month, 1);
                var fileBytes = await _reportingService.GenerateNDICPremiumReturnAsync(reportingPeriod);
                
                return File(
                    fileBytes, 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    $"NDIC_Premium_Return_{year}_Q{quarter}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<object>
                {
                    Success = false,
                    Message = $"Error generating NDIC Premium Return: {ex.Message}"
                });
            }
        }

        [HttpGet("ndic/deposit/{year}/{month}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,ReportingManager,ReportingOfficer")]
        public async Task<ActionResult> GenerateNDICDepositReturns(int year, int month)
        {
            try
            {
                if (month < 1 || month > 12)
                {
                    return BadRequest(new BaseResponse<object>
                    {
                        Success = false,
                        Message = "Month must be between 1 and 12"
                    });
                }
                
                var reportingPeriod = new DateTime(year, month, 1);
                var fileBytes = await _reportingService.GenerateNDICDepositReturnsAsync(reportingPeriod);
                
                return File(
                    fileBytes, 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    $"NDIC_Deposit_Return_{year}_{month:D2}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<object>
                {
                    Success = false,
                    Message = $"Error generating NDIC Deposit Returns: {ex.Message}"
                });
            }
        }

        [HttpGet("firs/monthly/{year}/{month}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,ReportingManager,ReportingOfficer")]
        public async Task<ActionResult> GenerateFIRSMonthlyTaxReturn(int year, int month)
        {
            try
            {
                if (month < 1 || month > 12)
                {
                    return BadRequest(new BaseResponse<object>
                    {
                        Success = false,
                        Message = "Month must be between 1 and 12"
                    });
                }
                
                var reportingPeriod = new DateTime(year, month, 1);
                var fileBytes = await _reportingService.GenerateFIRSMonthlyTaxReturnAsync(reportingPeriod);
                
                return File(
                    fileBytes, 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    $"FIRS_Monthly_Tax_Return_{year}_{month:D2}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<object>
                {
                    Success = false,
                    Message = $"Error generating FIRS Monthly Tax Return: {ex.Message}"
                });
            }
        }

        [HttpGet("firs/annual/{year}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,ReportingManager,ReportingOfficer")]
        public async Task<ActionResult> GenerateFIRSAnnualReturn(int year)
        {
            try
            {
                var fileBytes = await _reportingService.GenerateFIRSAnnualReturnAsync(year);
                
                return File(
                    fileBytes, 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    $"FIRS_Annual_Return_{year}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<object>
                {
                    Success = false,
                    Message = $"Error generating FIRS Annual Return: {ex.Message}"
                });
            }
        }

        #endregion

        #region Schedules

        [HttpGet("schedules")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin,ReportingManager,ReportingOfficer")]
        public async Task<ActionResult<BaseResponse<IEnumerable<RegulatoryReportScheduleDto>>>> GetUpcomingSchedules([FromQuery] int daysAhead = 30)
        {
            var schedules = await _reportingService.GetUpcomingSchedulesAsync(daysAhead);
            var schedulesDto = _mapper.Map<IEnumerable<RegulatoryReportScheduleDto>>(schedules);
            
            return Ok(new BaseResponse<IEnumerable<RegulatoryReportScheduleDto>>
            {
                Success = true,
                Message = $"Upcoming schedules for the next {daysAhead} days retrieved successfully",
                Data = schedulesDto
            });
        }

        [HttpPost("schedules")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,ReportingManager")]
        public async Task<ActionResult<BaseResponse<RegulatoryReportScheduleDto>>> CreateSchedule([FromBody] CreateRegulatoryReportScheduleDto createDto)
        {
            try
            {
                var schedule = _mapper.Map<RegulatoryReportSchedule>(createDto);
                var createdSchedule = await _reportingService.CreateScheduleAsync(schedule);
                var scheduleDto = _mapper.Map<RegulatoryReportScheduleDto>(createdSchedule);
                
                return CreatedAtAction(nameof(GetUpcomingSchedules), null, new BaseResponse<RegulatoryReportScheduleDto>
                {
                    Success = true,
                    Message = "Schedule created successfully",
                    Data = scheduleDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<RegulatoryReportScheduleDto>
                {
                    Success = false,
                    Message = $"Error creating schedule: {ex.Message}"
                });
            }
        }

        [HttpPut("schedules/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin,ReportingManager")]
        public async Task<ActionResult<BaseResponse<RegulatoryReportScheduleDto>>> UpdateSchedule(Guid id, [FromBody] UpdateRegulatoryReportScheduleDto updateDto)
        {
            if (id != updateDto.Id)
            {
                return BadRequest(new BaseResponse<RegulatoryReportScheduleDto>
                {
                    Success = false,
                    Message = "ID mismatch between route and body"
                });
            }
            
            try
            {
                var schedule = _mapper.Map<RegulatoryReportSchedule>(updateDto);
                var updatedSchedule = await _reportingService.UpdateScheduleAsync(schedule);
                var scheduleDto = _mapper.Map<RegulatoryReportScheduleDto>(updatedSchedule);
                
                return Ok(new BaseResponse<RegulatoryReportScheduleDto>
                {
                    Success = true,
                    Message = "Schedule updated successfully",
                    Data = scheduleDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<RegulatoryReportScheduleDto>
                {
                    Success = false,
                    Message = $"Error updating schedule: {ex.Message}"
                });
            }
        }

        [HttpDelete("schedules/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin,ReportingManager")]
        public async Task<ActionResult<BaseResponse<bool>>> DeleteSchedule(Guid id)
        {
            try
            {
                var result = await _reportingService.DeleteScheduleAsync(id);
                
                return Ok(new BaseResponse<bool>
                {
                    Success = true,
                    Message = "Schedule deleted successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<bool>
                {
                    Success = false,
                    Message = $"Error deleting schedule: {ex.Message}"
                });
            }
        }

        #endregion
    }
}