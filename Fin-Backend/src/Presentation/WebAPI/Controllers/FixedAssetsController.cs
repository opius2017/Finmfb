using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Application.DTOs.Common;
using FinTech.Core.Application.DTOs.FixedAssets;
using FinTech.Core.Application.Services;
using FinTech.Domain.Entities.FixedAssets;
using FinTech.Domain.Enums.FixedAssets;

namespace FinTech.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FixedAssetsController : ControllerBase
    {
        private readonly IFixedAssetService _fixedAssetService;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTimeService _dateTimeService;

        public FixedAssetsController(
            IFixedAssetService fixedAssetService,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IDateTimeService dateTimeService)
        {
            _fixedAssetService = fixedAssetService ?? throw new ArgumentNullException(nameof(fixedAssetService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }

        #region Asset Endpoints

        [HttpGet("assets")]
        public async Task<ActionResult<BaseResponse<IEnumerable<AssetDto>>>> GetAssets([FromQuery] AssetStatus? status)
        {
            try
            {
                IEnumerable<Asset> assets;
                if (status.HasValue)
                {
                    assets = await _fixedAssetService.GetAssetsByStatusAsync(status.Value);
                }
                else
                {
                    // Default to active assets if no status provided
                    assets = await _fixedAssetService.GetAssetsByStatusAsync(AssetStatus.Active);
                }

                var assetDtos = _mapper.Map<IEnumerable<AssetDto>>(assets);
                return Ok(new BaseResponse<IEnumerable<AssetDto>>
                {
                    Success = true,
                    Data = assetDtos,
                    Message = "Assets retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<IEnumerable<AssetDto>>
                {
                    Success = false,
                    Message = $"Error retrieving assets: {ex.Message}"
                });
            }
        }

        [HttpGet("assets/{id}")]
        public async Task<ActionResult<BaseResponse<AssetDto>>> GetAssetById(Guid id)
        {
            try
            {
                var asset = await _fixedAssetService.GetAssetByIdAsync(id);
                if (asset == null)
                {
                    return NotFound(new BaseResponse<AssetDto>
                    {
                        Success = false,
                        Message = $"Asset with ID {id} not found"
                    });
                }

                var assetDto = _mapper.Map<AssetDto>(asset);
                return Ok(new BaseResponse<AssetDto>
                {
                    Success = true,
                    Data = assetDto,
                    Message = "Asset retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<AssetDto>
                {
                    Success = false,
                    Message = $"Error retrieving asset: {ex.Message}"
                });
            }
        }

        [HttpGet("assets/number/{assetNumber}")]
        public async Task<ActionResult<BaseResponse<AssetDto>>> GetAssetByNumber(string assetNumber)
        {
            try
            {
                var asset = await _fixedAssetService.GetAssetByNumberAsync(assetNumber);
                if (asset == null)
                {
                    return NotFound(new BaseResponse<AssetDto>
                    {
                        Success = false,
                        Message = $"Asset with number {assetNumber} not found"
                    });
                }

                var assetDto = _mapper.Map<AssetDto>(asset);
                return Ok(new BaseResponse<AssetDto>
                {
                    Success = true,
                    Data = assetDto,
                    Message = "Asset retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<AssetDto>
                {
                    Success = false,
                    Message = $"Error retrieving asset: {ex.Message}"
                });
            }
        }

        [HttpGet("assets/category/{categoryId}")]
        public async Task<ActionResult<BaseResponse<IEnumerable<AssetDto>>>> GetAssetsByCategory(Guid categoryId)
        {
            try
            {
                var assets = await _fixedAssetService.GetAssetsByCategoryAsync(categoryId);
                var assetDtos = _mapper.Map<IEnumerable<AssetDto>>(assets);
                return Ok(new BaseResponse<IEnumerable<AssetDto>>
                {
                    Success = true,
                    Data = assetDtos,
                    Message = "Assets retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<IEnumerable<AssetDto>>
                {
                    Success = false,
                    Message = $"Error retrieving assets: {ex.Message}"
                });
            }
        }

        [HttpGet("assets/department/{department}")]
        public async Task<ActionResult<BaseResponse<IEnumerable<AssetDto>>>> GetAssetsByDepartment(string department)
        {
            try
            {
                var assets = await _fixedAssetService.GetAssetsByDepartmentAsync(department);
                var assetDtos = _mapper.Map<IEnumerable<AssetDto>>(assets);
                return Ok(new BaseResponse<IEnumerable<AssetDto>>
                {
                    Success = true,
                    Data = assetDtos,
                    Message = "Assets retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<IEnumerable<AssetDto>>
                {
                    Success = false,
                    Message = $"Error retrieving assets: {ex.Message}"
                });
            }
        }

        [HttpGet("assets/location/{location}")]
        public async Task<ActionResult<BaseResponse<IEnumerable<AssetDto>>>> GetAssetsByLocation(string location)
        {
            try
            {
                var assets = await _fixedAssetService.GetAssetsByLocationAsync(location);
                var assetDtos = _mapper.Map<IEnumerable<AssetDto>>(assets);
                return Ok(new BaseResponse<IEnumerable<AssetDto>>
                {
                    Success = true,
                    Data = assetDtos,
                    Message = "Assets retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<IEnumerable<AssetDto>>
                {
                    Success = false,
                    Message = $"Error retrieving assets: {ex.Message}"
                });
            }
        }

        [HttpPost("assets")]
        public async Task<ActionResult<BaseResponse<AssetDto>>> CreateAsset([FromBody] CreateAssetDto createAssetDto)
        {
            try
            {
                var asset = _mapper.Map<Asset>(createAssetDto);
                
                // Set additional properties not in the DTO
                var tenantId = await _currentUserService.GetCurrentTenantId();
                asset.TenantId = tenantId;

                var createdAsset = await _fixedAssetService.CreateAssetAsync(asset);
                var assetDto = _mapper.Map<AssetDto>(createdAsset);

                return CreatedAtAction(nameof(GetAssetById), new { id = createdAsset.Id }, new BaseResponse<AssetDto>
                {
                    Success = true,
                    Data = assetDto,
                    Message = "Asset created successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<AssetDto>
                {
                    Success = false,
                    Message = $"Error creating asset: {ex.Message}"
                });
            }
        }

        [HttpPut("assets/{id}")]
        public async Task<ActionResult<BaseResponse<AssetDto>>> UpdateAsset(Guid id, [FromBody] UpdateAssetDto updateAssetDto)
        {
            try
            {
                if (id != updateAssetDto.Id)
                {
                    return BadRequest(new BaseResponse<AssetDto>
                    {
                        Success = false,
                        Message = "ID mismatch"
                    });
                }

                var existingAsset = await _fixedAssetService.GetAssetByIdAsync(id);
                if (existingAsset == null)
                {
                    return NotFound(new BaseResponse<AssetDto>
                    {
                        Success = false,
                        Message = $"Asset with ID {id} not found"
                    });
                }

                // Update only the fields allowed to be updated
                existingAsset.AssetName = updateAssetDto.AssetName;
                existingAsset.Description = updateAssetDto.Description;
                existingAsset.Location = updateAssetDto.Location;
                existingAsset.Department = updateAssetDto.Department;
                existingAsset.CustodianId = updateAssetDto.CustodianId;
                existingAsset.AssetTag = updateAssetDto.AssetTag;
                existingAsset.SerialNumber = updateAssetDto.SerialNumber;
                existingAsset.Notes = updateAssetDto.Notes;
                existingAsset.WarrantyExpiryDate = updateAssetDto.WarrantyExpiryDate;

                var updatedAsset = await _fixedAssetService.UpdateAssetAsync(existingAsset);
                var assetDto = _mapper.Map<AssetDto>(updatedAsset);

                return Ok(new BaseResponse<AssetDto>
                {
                    Success = true,
                    Data = assetDto,
                    Message = "Asset updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<AssetDto>
                {
                    Success = false,
                    Message = $"Error updating asset: {ex.Message}"
                });
            }
        }

        [HttpDelete("assets/{id}")]
        public async Task<ActionResult<BaseResponse<bool>>> DeleteAsset(Guid id)
        {
            try
            {
                var result = await _fixedAssetService.DeleteAssetAsync(id);
                if (!result)
                {
                    return NotFound(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = $"Asset with ID {id} not found"
                    });
                }

                return Ok(new BaseResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Asset deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = $"Error deleting asset: {ex.Message}"
                });
            }
        }

        #endregion

        #region Asset Category Endpoints

        [HttpGet("categories")]
        public async Task<ActionResult<BaseResponse<IEnumerable<AssetCategoryDto>>>> GetAssetCategories()
        {
            try
            {
                var categories = await _fixedAssetService.GetAllAssetCategoriesAsync();
                var categoryDtos = _mapper.Map<IEnumerable<AssetCategoryDto>>(categories);
                return Ok(new BaseResponse<IEnumerable<AssetCategoryDto>>
                {
                    Success = true,
                    Data = categoryDtos,
                    Message = "Asset categories retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<IEnumerable<AssetCategoryDto>>
                {
                    Success = false,
                    Message = $"Error retrieving asset categories: {ex.Message}"
                });
            }
        }

        [HttpGet("categories/{id}")]
        public async Task<ActionResult<BaseResponse<AssetCategoryDto>>> GetAssetCategoryById(Guid id)
        {
            try
            {
                var category = await _fixedAssetService.GetAssetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new BaseResponse<AssetCategoryDto>
                    {
                        Success = false,
                        Message = $"Asset category with ID {id} not found"
                    });
                }

                var categoryDto = _mapper.Map<AssetCategoryDto>(category);
                return Ok(new BaseResponse<AssetCategoryDto>
                {
                    Success = true,
                    Data = categoryDto,
                    Message = "Asset category retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<AssetCategoryDto>
                {
                    Success = false,
                    Message = $"Error retrieving asset category: {ex.Message}"
                });
            }
        }

        [HttpGet("categories/code/{categoryCode}")]
        public async Task<ActionResult<BaseResponse<AssetCategoryDto>>> GetAssetCategoryByCode(string categoryCode)
        {
            try
            {
                var category = await _fixedAssetService.GetAssetCategoryByCodeAsync(categoryCode);
                if (category == null)
                {
                    return NotFound(new BaseResponse<AssetCategoryDto>
                    {
                        Success = false,
                        Message = $"Asset category with code {categoryCode} not found"
                    });
                }

                var categoryDto = _mapper.Map<AssetCategoryDto>(category);
                return Ok(new BaseResponse<AssetCategoryDto>
                {
                    Success = true,
                    Data = categoryDto,
                    Message = "Asset category retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<AssetCategoryDto>
                {
                    Success = false,
                    Message = $"Error retrieving asset category: {ex.Message}"
                });
            }
        }

        [HttpPost("categories")]
        public async Task<ActionResult<BaseResponse<AssetCategoryDto>>> CreateAssetCategory([FromBody] CreateAssetCategoryDto createCategoryDto)
        {
            try
            {
                var category = _mapper.Map<AssetCategory>(createCategoryDto);
                
                // Set additional properties not in the DTO
                var tenantId = await _currentUserService.GetCurrentTenantId();
                category.TenantId = tenantId;

                var createdCategory = await _fixedAssetService.CreateAssetCategoryAsync(category);
                var categoryDto = _mapper.Map<AssetCategoryDto>(createdCategory);

                return CreatedAtAction(nameof(GetAssetCategoryById), new { id = createdCategory.Id }, new BaseResponse<AssetCategoryDto>
                {
                    Success = true,
                    Data = categoryDto,
                    Message = "Asset category created successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<AssetCategoryDto>
                {
                    Success = false,
                    Message = $"Error creating asset category: {ex.Message}"
                });
            }
        }

        [HttpPut("categories/{id}")]
        public async Task<ActionResult<BaseResponse<AssetCategoryDto>>> UpdateAssetCategory(Guid id, [FromBody] UpdateAssetCategoryDto updateCategoryDto)
        {
            try
            {
                if (id != updateCategoryDto.Id)
                {
                    return BadRequest(new BaseResponse<AssetCategoryDto>
                    {
                        Success = false,
                        Message = "ID mismatch"
                    });
                }

                var existingCategory = await _fixedAssetService.GetAssetCategoryByIdAsync(id);
                if (existingCategory == null)
                {
                    return NotFound(new BaseResponse<AssetCategoryDto>
                    {
                        Success = false,
                        Message = $"Asset category with ID {id} not found"
                    });
                }

                // Update only the fields allowed to be updated
                existingCategory.CategoryName = updateCategoryDto.CategoryName;
                existingCategory.Description = updateCategoryDto.Description;
                existingCategory.ParentCategoryId = updateCategoryDto.ParentCategoryId;
                existingCategory.DefaultDepreciationMethod = updateCategoryDto.DefaultDepreciationMethod;
                existingCategory.DefaultUsefulLifeYears = updateCategoryDto.DefaultUsefulLifeYears;
                existingCategory.DefaultSalvageValuePercent = updateCategoryDto.DefaultSalvageValuePercent;
                existingCategory.AssetAccountId = updateCategoryDto.AssetAccountId;
                existingCategory.DepreciationExpenseAccountId = updateCategoryDto.DepreciationExpenseAccountId;
                existingCategory.AccumulatedDepreciationAccountId = updateCategoryDto.AccumulatedDepreciationAccountId;

                var updatedCategory = await _fixedAssetService.UpdateAssetCategoryAsync(existingCategory);
                var categoryDto = _mapper.Map<AssetCategoryDto>(updatedCategory);

                return Ok(new BaseResponse<AssetCategoryDto>
                {
                    Success = true,
                    Data = categoryDto,
                    Message = "Asset category updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<AssetCategoryDto>
                {
                    Success = false,
                    Message = $"Error updating asset category: {ex.Message}"
                });
            }
        }

        [HttpDelete("categories/{id}")]
        public async Task<ActionResult<BaseResponse<bool>>> DeleteAssetCategory(Guid id)
        {
            try
            {
                var result = await _fixedAssetService.DeleteAssetCategoryAsync(id);
                if (!result)
                {
                    return NotFound(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = $"Asset category with ID {id} not found"
                    });
                }

                return Ok(new BaseResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Asset category deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = $"Error deleting asset category: {ex.Message}"
                });
            }
        }

        #endregion

        #region Depreciation Endpoints

        [HttpGet("assets/{assetId}/depreciation-schedule")]
        public async Task<ActionResult<BaseResponse<IEnumerable<AssetDepreciationScheduleDto>>>> GetDepreciationSchedule(Guid assetId)
        {
            try
            {
                var schedules = await _fixedAssetService.GetDepreciationScheduleForAssetAsync(assetId);
                var scheduleDtos = _mapper.Map<IEnumerable<AssetDepreciationScheduleDto>>(schedules);
                return Ok(new BaseResponse<IEnumerable<AssetDepreciationScheduleDto>>
                {
                    Success = true,
                    Data = scheduleDtos,
                    Message = "Depreciation schedule retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<IEnumerable<AssetDepreciationScheduleDto>>
                {
                    Success = false,
                    Message = $"Error retrieving depreciation schedule: {ex.Message}"
                });
            }
        }

        [HttpPost("assets/{assetId}/depreciation-schedule")]
        public async Task<ActionResult<BaseResponse<IEnumerable<AssetDepreciationScheduleDto>>>> GenerateDepreciationSchedule(Guid assetId)
        {
            try
            {
                var schedules = await _fixedAssetService.GenerateDepreciationScheduleAsync(assetId);
                var scheduleDtos = _mapper.Map<IEnumerable<AssetDepreciationScheduleDto>>(schedules);
                return Ok(new BaseResponse<IEnumerable<AssetDepreciationScheduleDto>>
                {
                    Success = true,
                    Data = scheduleDtos,
                    Message = "Depreciation schedule generated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<IEnumerable<AssetDepreciationScheduleDto>>
                {
                    Success = false,
                    Message = $"Error generating depreciation schedule: {ex.Message}"
                });
            }
        }

        [HttpPost("depreciation/process")]
        public async Task<ActionResult<BaseResponse<bool>>> ProcessMonthlyDepreciation([FromQuery] DateTime? asOfDate)
        {
            try
            {
                var date = asOfDate ?? _dateTimeService.Now;
                var result = await _fixedAssetService.ProcessMonthlyDepreciationAsync(date);
                return Ok(new BaseResponse<bool>
                {
                    Success = true,
                    Data = result,
                    Message = "Monthly depreciation processed successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = $"Error processing monthly depreciation: {ex.Message}"
                });
            }
        }

        [HttpPost("depreciation/post-to-gl")]
        public async Task<ActionResult<BaseResponse<bool>>> PostDepreciationToGL([FromQuery] DateTime periodStart, [FromQuery] DateTime periodEnd)
        {
            try
            {
                var result = await _fixedAssetService.PostDepreciationToGLAsync(periodStart, periodEnd);
                return Ok(new BaseResponse<bool>
                {
                    Success = true,
                    Data = result,
                    Message = "Depreciation posted to GL successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = $"Error posting depreciation to GL: {ex.Message}"
                });
            }
        }

        #endregion

        #region Asset Maintenance Endpoints

        [HttpGet("maintenance")]
        public async Task<ActionResult<BaseResponse<IEnumerable<AssetMaintenanceDto>>>> GetScheduledMaintenance(
            [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var from = fromDate ?? _dateTimeService.Now;
                var to = toDate ?? from.AddMonths(1);
                
                var maintenanceRecords = await _fixedAssetService.GetScheduledMaintenanceAsync(from, to);
                var maintenanceDtos = _mapper.Map<IEnumerable<AssetMaintenanceDto>>(maintenanceRecords);
                
                return Ok(new BaseResponse<IEnumerable<AssetMaintenanceDto>>
                {
                    Success = true,
                    Data = maintenanceDtos,
                    Message = "Scheduled maintenance retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<IEnumerable<AssetMaintenanceDto>>
                {
                    Success = false,
                    Message = $"Error retrieving scheduled maintenance: {ex.Message}"
                });
            }
        }

        [HttpGet("assets/{assetId}/maintenance")]
        public async Task<ActionResult<BaseResponse<IEnumerable<AssetMaintenanceDto>>>> GetMaintenanceHistory(Guid assetId)
        {
            try
            {
                var maintenanceRecords = await _fixedAssetService.GetMaintenanceHistoryForAssetAsync(assetId);
                var maintenanceDtos = _mapper.Map<IEnumerable<AssetMaintenanceDto>>(maintenanceRecords);
                
                return Ok(new BaseResponse<IEnumerable<AssetMaintenanceDto>>
                {
                    Success = true,
                    Data = maintenanceDtos,
                    Message = "Maintenance history retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<IEnumerable<AssetMaintenanceDto>>
                {
                    Success = false,
                    Message = $"Error retrieving maintenance history: {ex.Message}"
                });
            }
        }

        [HttpGet("maintenance/{id}")]
        public async Task<ActionResult<BaseResponse<AssetMaintenanceDto>>> GetMaintenanceById(Guid id)
        {
            try
            {
                var maintenance = await _fixedAssetService.GetMaintenanceRecordByIdAsync(id);
                if (maintenance == null)
                {
                    return NotFound(new BaseResponse<AssetMaintenanceDto>
                    {
                        Success = false,
                        Message = $"Maintenance record with ID {id} not found"
                    });
                }

                var maintenanceDto = _mapper.Map<AssetMaintenanceDto>(maintenance);
                return Ok(new BaseResponse<AssetMaintenanceDto>
                {
                    Success = true,
                    Data = maintenanceDto,
                    Message = "Maintenance record retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<AssetMaintenanceDto>
                {
                    Success = false,
                    Message = $"Error retrieving maintenance record: {ex.Message}"
                });
            }
        }

        [HttpPost("maintenance")]
        public async Task<ActionResult<BaseResponse<AssetMaintenanceDto>>> CreateMaintenance([FromBody] CreateAssetMaintenanceDto createMaintenanceDto)
        {
            try
            {
                var maintenance = _mapper.Map<AssetMaintenance>(createMaintenanceDto);
                
                var createdMaintenance = await _fixedAssetService.CreateMaintenanceRecordAsync(maintenance);
                var maintenanceDto = _mapper.Map<AssetMaintenanceDto>(createdMaintenance);

                return CreatedAtAction(nameof(GetMaintenanceById), new { id = createdMaintenance.Id }, new BaseResponse<AssetMaintenanceDto>
                {
                    Success = true,
                    Data = maintenanceDto,
                    Message = "Maintenance record created successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<AssetMaintenanceDto>
                {
                    Success = false,
                    Message = $"Error creating maintenance record: {ex.Message}"
                });
            }
        }

        [HttpPut("maintenance/{id}")]
        public async Task<ActionResult<BaseResponse<AssetMaintenanceDto>>> UpdateMaintenance(Guid id, [FromBody] UpdateAssetMaintenanceDto updateMaintenanceDto)
        {
            try
            {
                if (id != updateMaintenanceDto.Id)
                {
                    return BadRequest(new BaseResponse<AssetMaintenanceDto>
                    {
                        Success = false,
                        Message = "ID mismatch"
                    });
                }

                var existingMaintenance = await _fixedAssetService.GetMaintenanceRecordByIdAsync(id);
                if (existingMaintenance == null)
                {
                    return NotFound(new BaseResponse<AssetMaintenanceDto>
                    {
                        Success = false,
                        Message = $"Maintenance record with ID {id} not found"
                    });
                }

                // Update fields
                existingMaintenance.MaintenanceDate = updateMaintenanceDto.MaintenanceDate;
                existingMaintenance.MaintenanceType = updateMaintenanceDto.MaintenanceType;
                existingMaintenance.Status = updateMaintenanceDto.Status;
                existingMaintenance.Description = updateMaintenanceDto.Description;
                existingMaintenance.VendorId = updateMaintenanceDto.VendorId;
                existingMaintenance.Cost = updateMaintenanceDto.Cost;
                existingMaintenance.Notes = updateMaintenanceDto.Notes;

                var updatedMaintenance = await _fixedAssetService.UpdateMaintenanceRecordAsync(existingMaintenance);
                var maintenanceDto = _mapper.Map<AssetMaintenanceDto>(updatedMaintenance);

                return Ok(new BaseResponse<AssetMaintenanceDto>
                {
                    Success = true,
                    Data = maintenanceDto,
                    Message = "Maintenance record updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<AssetMaintenanceDto>
                {
                    Success = false,
                    Message = $"Error updating maintenance record: {ex.Message}"
                });
            }
        }

        [HttpDelete("maintenance/{id}")]
        public async Task<ActionResult<BaseResponse<bool>>> DeleteMaintenance(Guid id)
        {
            try
            {
                var result = await _fixedAssetService.DeleteMaintenanceRecordAsync(id);
                if (!result)
                {
                    return NotFound(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = $"Maintenance record with ID {id} not found"
                    });
                }

                return Ok(new BaseResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Maintenance record deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = $"Error deleting maintenance record: {ex.Message}"
                });
            }
        }

        #endregion

        // Additional endpoints for Asset Transfers, Inventory Counts, 
        // Asset Disposals, and Asset Revaluations would follow the same pattern
        // but are omitted here for brevity.
    }
}