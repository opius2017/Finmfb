using AutoMapper;
using FinTech.Core.Application.DTOs.FixedAssets;
using FinTech.Core.Domain.Entities.FixedAssets;

namespace FinTech.Core.Application.Mappings
{
    public class FixedAssetMappingProfile : Profile
    {
        public FixedAssetMappingProfile()
        {
            // Asset mappings
            CreateMap<Asset, AssetDto>()
                .ForMember(dest => dest.AssetCategoryName, opt => opt.MapFrom(src => src.AssetCategory.CategoryName))
                .ForMember(dest => dest.CustodianName, opt => opt.MapFrom(src => src.Custodian));
            
            CreateMap<CreateAssetDto, Asset>();
            CreateMap<UpdateAssetDto, Asset>();
            
            // Asset Category mappings
            CreateMap<AssetCategory, AssetCategoryDto>()
                .ForMember(dest => dest.ParentCategoryName, opt => opt.Ignore());
            
            CreateMap<CreateAssetCategoryDto, AssetCategory>();
            CreateMap<UpdateAssetCategoryDto, AssetCategory>();
            
            // Asset Depreciation Schedule mappings
            CreateMap<AssetDepreciationSchedule, AssetDepreciationScheduleDto>()
                .ForMember(dest => dest.AssetNumber, opt => opt.MapFrom(src => src.Asset.AssetNumber))
                .ForMember(dest => dest.AssetName, opt => opt.MapFrom(src => src.Asset.AssetName));
            
            // Asset Maintenance mappings
            CreateMap<AssetMaintenance, AssetMaintenanceDto>()
                .ForMember(dest => dest.AssetNumber, opt => opt.MapFrom(src => src.Asset.AssetNumber))
                .ForMember(dest => dest.AssetName, opt => opt.MapFrom(src => src.Asset.AssetName));
            
            CreateMap<CreateAssetMaintenanceDto, AssetMaintenance>();
            CreateMap<UpdateAssetMaintenanceDto, AssetMaintenance>();
            
            // Additional mappings for Asset Transfer, Inventory Count, Disposal, and Revaluation would follow the same pattern
        }
    }
}
