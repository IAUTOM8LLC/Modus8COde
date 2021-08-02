using AutoMapper;
using IAutoM8.Domain.Models.Resource;
using IAutoM8.Global.Enums;
using IAutoM8.Neo4jRepository.Dto;
using IAutoM8.Service.Resources.Dto;
using System;
using System.Collections.Generic;

namespace IAutoM8.Service.Infrastructure.MapperProfiles
{
    public class ResourceProfile : Profile
    {
        public ResourceProfile()
        {
            ToDto();
            ToNeo4jDto();
            FromNeo4jDto();
        }

        private void FromNeo4jDto()
        {
            CreateMap<TaskResourceNeo4jDto, ResourceDto>()
                .ForMember(dest => dest.Url,
                    opt => opt.ResolveUsing((source, destination, member, context)
                        => source.Type == (byte)ResourceType.File ?
                        ((Func<string, string>)context.Items["urlBuilder"])(source.Path) : source.Path));
            CreateMap<TaskResourceInfoNeo4jDto, ResourceInfoDto>()
                .ForMember(dest => dest.Url,
                    opt => opt.ResolveUsing((source, destination, member, context)
                        => source.Type == (byte)ResourceType.File ?
                        ((Func<string, string>)context.Items["urlBuilder"])(source.Path) : source.Path));
        }

        private void ToNeo4jDto()
        {
            CreateMap<TaskResourceNeo4jDto, TaskResourceNeo4jDto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Path,
                    opt => opt.ResolveUsing((source, destination, member, context) => (string)context.Items["path"]));

            CreateMap<UrlResourceDto, TaskResourceNeo4jDto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsSharedFromParent, opt => opt.Ignore())
                .ForMember(dest => dest.CameFromParent, opt => opt.Ignore())
                .ForMember(dest => dest.Mime, opt => opt.Ignore())
                .ForMember(dest => dest.Size, opt => opt.Ignore())
                .ForMember(dest => dest.IsPublished, opt => opt.Ignore())
                .ForMember(dest => dest.OriginType, opt => opt.Ignore())
                .ForMember(dest => dest.TimeStamp, opt => opt.Ignore())
                .ForMember(dest => dest.Path,
                    opt => opt.MapFrom(src => src.Url))
                .ForMember(dest => dest.Name,
                    opt => opt.MapFrom(src => src.Url))
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => (byte)ResourceType.Link));

            CreateMap<FileResourceDto, TaskResourceNeo4jDto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsSharedFromParent, opt => opt.Ignore())
                .ForMember(dest => dest.CameFromParent, opt => opt.Ignore())
                .ForMember(dest => dest.IsPublished, opt => opt.Ignore())
                .ForMember(dest => dest.OriginType,
                    opt => opt.MapFrom(src => src.OriginType))
               .ForMember(dest => dest.TimeStamp,
                    opt => opt.MapFrom(src => src.TimeStamp))
                .ForMember(dest => dest.Path,
                    opt => opt.ResolveUsing((source, destination, member, context)
                        => ((Func<string, string>)context.Items["pathBuilder"])(source.Name)))
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => (byte)ResourceType.File));

            CreateMap<UpdateResourceItemDto, UpdateTaskResourceNeo4jDto>();
        }

        private void ToDto()
        {
            CreateMap<Resource, Resource>()
                .ForMember(dest => dest.Id,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ResourceFormula,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ResourceFormulaTask,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ResourceProject,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ResourceProjectTask,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Path,
                    opt => opt.ResolveUsing((source, destination, member, context) =>
                    context.Items.ContainsKey("path") ? (string)context.Items["path"] : source.Path));

            CreateMap<Resource, ResourceDto>()
                .ForMember(dest => dest.Url,
                    opt => opt.ResolveUsing((source, destination, member, context)
                        => source.Type == ResourceType.File ?
                        ((Func<string, string>)context.Items["urlBuilder"])(source.Path) : source.Path))
                .ForMember(dest => dest.IsSharedFromParent,
                    opt => opt.ResolveUsing((source, destination, member, context)
                        => context.Items.ContainsKey("isShared") && (bool)context.Items["isShared"]))
                .ForMember(dest => dest.IsGlobalShared, opt => opt.UseValue(false))
                .ForMember(dest => dest.IsShared, opt => opt.UseValue(false))
                .ForMember(dest => dest.IsPublished, opt => opt.Ignore())
                .ForMember(dest => dest.OriginType,
                 opt => opt.MapFrom(source => Enum.GetName(typeof(ResourceOriginType), source.OriginType)))
                .ForMember(dest => dest.TimeStamp, opt => opt.Ignore())
                .ForMember(dest => dest.CameFromParent, opt => opt.UseValue(false));

            CreateMap<Resource, ResourceInfoDto>()
                .ForMember(dest => dest.Url,
                    opt => opt.ResolveUsing((source, destination, member, context)
                        => source.Type == ResourceType.File ?
                        ((Func<string, string>)context.Items["urlBuilder"])(source.Path) : source.Path))
                .ForMember(dest => dest.IsGlobalShared,
                    opt => opt.UseValue(true))
                .ForMember(dest => dest.TaskIds, opt => opt.UseValue(new List<int>()));

        }
    }
}
