using AutoMapper;
using FinTech.Core.Application.DTOs.RegulatoryReporting;
using FinTech.Core.Domain.Entities.RegulatoryReporting;
using System.Linq;

namespace FinTech.Core.Application.Mappings
{
    public class RegulatoryReportingMappingProfile : Profile
    {
        public RegulatoryReportingMappingProfile()
        {
            // Template mappings
            CreateMap<RegulatoryReportTemplate, RegulatoryReportTemplateDto>()
                .ForMember(dest => dest.SectionCount, opt => opt.MapFrom(src => 0)) // Placeholder as Sections is likely JSON now
                .ForMember(dest => dest.FieldCount, opt => opt.MapFrom(src => 0)); // Placeholder
                
            CreateMap<RegulatoryReportTemplate, RegulatoryReportTemplateDetailDto>()
                .IncludeBase<RegulatoryReportTemplate, RegulatoryReportTemplateDto>();
                
            CreateMap<CreateRegulatoryReportTemplateDto, RegulatoryReportTemplate>();
            CreateMap<UpdateRegulatoryReportTemplateDto, RegulatoryReportTemplate>();
            
            // Section mappings
            CreateMap<RegulatoryReportSection, RegulatoryReportSectionDto>();
            CreateMap<CreateRegulatoryReportSectionDto, RegulatoryReportSection>();
            CreateMap<UpdateRegulatoryReportSectionDto, RegulatoryReportSection>();
            
            // Field mappings
            CreateMap<RegulatoryReportField, RegulatoryReportFieldDto>();
            CreateMap<CreateRegulatoryReportFieldDto, RegulatoryReportField>();
            CreateMap<UpdateRegulatoryReportFieldDto, RegulatoryReportField>();
            
            // Submission mappings
            CreateMap<RegulatoryReportSubmission, RegulatoryReportSubmissionDto>()
                .ForMember(dest => dest.TemplateName, opt => opt.MapFrom(src => src.Template != null ? src.Template.TemplateName : null))
                .ForMember(dest => dest.TemplateCode, opt => opt.MapFrom(src => src.Template != null ? src.Template.TemplateCode : null))
                .ForMember(dest => dest.RegulatoryBody, opt => opt.MapFrom(src => src.Template != null ? src.Template.RegulatoryBody : default))
                .ForMember(dest => dest.ValidationErrorCount, opt => opt.MapFrom(src => 
                    src.ReportData != null 
                        ? src.ReportData.Count(d => d.HasValidationErrors) 
                        : 0))
                .ForMember(dest => dest.ValidationWarningCount, opt => opt.MapFrom(src => 0));
                
            CreateMap<RegulatoryReportSubmission, RegulatoryReportSubmissionDetailDto>()
                .IncludeBase<RegulatoryReportSubmission, RegulatoryReportSubmissionDto>();
                
            CreateMap<CreateRegulatoryReportSubmissionDto, RegulatoryReportSubmission>();
            
            // Report Data mappings
            CreateMap<RegulatoryReportData, RegulatoryReportDataDto>()
                .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => src.FieldName))
                .ForMember(dest => dest.FieldCode, opt => opt.MapFrom(src => src.FieldCode))
                .ForMember(dest => dest.SectionName, opt => opt.MapFrom(src => src.SectionName))
                .ForMember(dest => dest.DataType, opt => opt.MapFrom(src => src.DataType))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.RawValue));
                
            CreateMap<UpdateRegulatoryReportDataDto, RegulatoryReportData>();
            
            // Validation mappings
            CreateMap<RegulatoryReportValidation, RegulatoryReportValidationDto>()
                .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => src.FieldCode)); // Fallback to Code as Name is not directly available via nav property
                
            CreateMap<ResolveRegulatoryReportValidationDto, RegulatoryReportValidation>();
            
            // Schedule mappings
            CreateMap<RegulatoryReportSchedule, RegulatoryReportScheduleDto>()
                .ForMember(dest => dest.TemplateName, opt => opt.MapFrom(src => src.Template != null ? src.Template.TemplateName : null))
                .ForMember(dest => dest.TemplateCode, opt => opt.MapFrom(src => src.Template != null ? src.Template.TemplateCode : null))
                .ForMember(dest => dest.RegulatoryBody, opt => opt.MapFrom(src => src.Template != null ? src.Template.RegulatoryBody : default))
                .ForMember(dest => dest.Frequency, opt => opt.MapFrom(src => src.Template != null ? src.Template.Frequency : default));
                
            CreateMap<CreateRegulatoryReportScheduleDto, RegulatoryReportSchedule>();
            CreateMap<UpdateRegulatoryReportScheduleDto, RegulatoryReportSchedule>();
        }
    }
}
