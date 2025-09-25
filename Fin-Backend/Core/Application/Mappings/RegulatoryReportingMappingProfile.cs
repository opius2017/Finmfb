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
                .ForMember(dest => dest.SectionCount, opt => opt.MapFrom(src => src.Sections != null ? src.Sections.Count : 0))
                .ForMember(dest => dest.FieldCount, opt => opt.MapFrom(src => src.Sections != null ? src.Sections.Sum(s => s.Fields != null ? s.Fields.Count : 0) : 0));
                
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
                .ForMember(dest => dest.TemplateName, opt => opt.MapFrom(src => src.ReportTemplate != null ? src.ReportTemplate.TemplateName : null))
                .ForMember(dest => dest.TemplateCode, opt => opt.MapFrom(src => src.ReportTemplate != null ? src.ReportTemplate.TemplateCode : null))
                .ForMember(dest => dest.RegulatoryBody, opt => opt.MapFrom(src => src.ReportTemplate != null ? src.ReportTemplate.RegulatoryBody : default))
                .ForMember(dest => dest.ValidationErrorCount, opt => opt.MapFrom(src => 
                    src.ReportData != null 
                        ? src.ReportData.Count(d => d.HasException) 
                        : 0))
                .ForMember(dest => dest.ValidationWarningCount, opt => opt.MapFrom(src => 0)); // Placeholder for validation warnings
                
            CreateMap<RegulatoryReportSubmission, RegulatoryReportSubmissionDetailDto>()
                .IncludeBase<RegulatoryReportSubmission, RegulatoryReportSubmissionDto>();
                
            CreateMap<CreateRegulatoryReportSubmissionDto, RegulatoryReportSubmission>();
            
            // Report Data mappings
            CreateMap<RegulatoryReportData, RegulatoryReportDataDto>()
                .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => src.Field != null ? src.Field.FieldName : null))
                .ForMember(dest => dest.FieldCode, opt => opt.MapFrom(src => src.Field != null ? src.Field.FieldCode : null))
                .ForMember(dest => dest.SectionName, opt => opt.MapFrom(src => src.Field != null && src.Field.Section != null ? src.Field.Section.SectionName : null))
                .ForMember(dest => dest.DataType, opt => opt.MapFrom(src => src.Field != null ? src.Field.DataType : null));
                
            CreateMap<UpdateRegulatoryReportDataDto, RegulatoryReportData>();
            
            // Validation mappings
            CreateMap<RegulatoryReportValidation, RegulatoryReportValidationDto>()
                .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => src.Field != null ? src.Field.FieldName : null))
                .ForMember(dest => dest.FieldCode, opt => opt.MapFrom(src => src.Field != null ? src.Field.FieldCode : null));
                
            CreateMap<ResolveRegulatoryReportValidationDto, RegulatoryReportValidation>();
            
            // Schedule mappings
            CreateMap<RegulatoryReportSchedule, RegulatoryReportScheduleDto>()
                .ForMember(dest => dest.TemplateName, opt => opt.MapFrom(src => src.ReportTemplate != null ? src.ReportTemplate.TemplateName : null))
                .ForMember(dest => dest.TemplateCode, opt => opt.MapFrom(src => src.ReportTemplate != null ? src.ReportTemplate.TemplateCode : null))
                .ForMember(dest => dest.RegulatoryBody, opt => opt.MapFrom(src => src.ReportTemplate != null ? src.ReportTemplate.RegulatoryBody : default))
                .ForMember(dest => dest.Frequency, opt => opt.MapFrom(src => src.ReportTemplate != null ? src.ReportTemplate.Frequency : default));
                
            CreateMap<CreateRegulatoryReportScheduleDto, RegulatoryReportSchedule>();
            CreateMap<UpdateRegulatoryReportScheduleDto, RegulatoryReportSchedule>();
        }
    }
}
