using AutoMapper;
using IAutoM8.Service.Skills.Dto;
using System.Linq;
using IAutoM8.Domain.Models.Skill;
using IAutoM8.Global;
using System;
using IAutoM8.Global.Enums;
using IAutoM8.Service.Teams.Dto;
using IAutoM8.Domain.Models.Team;

namespace IAutoM8.Service.Infrastructure.MapperProfiles.Skill
{
    internal class SkillProfile : Profile
    {
        public SkillProfile()
        {
            ToDto();
            FromDto();
        }

        private void FromDto()
        {
            CreateMap<UpdateSkillDetailDto, Domain.Models.Skill.Skill>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.IsGlobal, opt => opt.MapFrom(src => src.IsGlobal))
                .ForMember(dest => dest.UserSkills, opt => opt.Ignore())
                .ForMember(dest => dest.Owner, opt => opt.Ignore())
                .ForMember(dest => dest.OwnerGuid, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewingTasks, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedTasks, opt => opt.Ignore())
                .ForAllOtherMembers(opt => opt.Ignore());
        }

        private void ToDto()
        {
            CreateMap<Domain.Models.Skill.Skill, SkillDetailDto>()
                .ForMember(dest => dest.UserSkills, opt => opt.MapFrom(src => src.UserSkills.Select(s => s.UserId)));

            CreateMap<Domain.Models.Skill.Skill, UpdateSkillDetailDto>()
                .ForMember(dest => dest.UserSkills, opt => opt.MapFrom(src => src.UserSkills.Select(s => s.UserId)))
                .ForMember(dest => dest.TeamId, opt => opt.Ignore());

            CreateMap<Domain.Models.Skill.Skill, SkillDto>()
                .ForMember(dest => dest.IsWorkerSkill, opt => opt.MapFrom(src => src.UserSkills.Any(x => x.User.Roles.Any(r => r.Role.Name == UserRoles.Worker))))
                .ForMember(dest => dest.HasAssignedTasks,
                    opt => opt.MapFrom(src => src.AssignedTasks.Count > 0
                        || src.ReviewingTasks.Count > 0
                        || src.AssignedFormulaTasks.Count > 0
                        || src.ReviewingFormulaTasks.Count > 0))
                .ForMember(dest => dest.Users, opt => opt.MapFrom(src => src.UserSkills))
                //.ForMember(dest => dest.Users,
                //    opt => opt.ResolveUsing((source, dest, member, context) =>
                //    {
                //        var companyUsers = ((IEnumerable<Guid>)context.Items["CompanyUsers"]);

                //        if (companyUsers != null && companyUsers.Count() > 0)
                //        {
                //            var users = source.UserSkills.Where(w => companyUsers.Contains(w.UserId)).ToList();
                //            return users ?? new List<UserSkill>();
                //        }

                //        return source.UserSkills;
                //    }))
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.TeamSkills.Select(x => x.Team.Name).FirstOrDefault()))
                .ForMember(dest => dest.RevFormulas, opt => opt.MapFrom(src => src.ReviewingFormulaTasks.Select(s => s.FormulaProject).ToList()))
                .ForMember(dest => dest.DevFormulas, opt => opt.MapFrom(src => src.AssignedFormulaTasks.Select(s => s.FormulaProject).ToList()));

            CreateMap<UserSkill, UserSkillDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Profile.FullName))
                .ForMember(dest => dest.UserImage, opt => opt.MapFrom(src => src.User.Profile.Path));

            CreateMap<Domain.Models.Skill.Skill, Domain.Models.Skill.Skill>()
               .ForMember(dest => dest.Owner, opt => opt.Ignore())
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.ReviewingFormulaTasks, opt => opt.Ignore())
               .ForMember(dest => dest.ReviewingTasks, opt => opt.Ignore())
               .ForMember(dest => dest.AssignedTasks, opt => opt.Ignore())
               .ForMember(dest => dest.AssignedFormulaTasks, opt => opt.Ignore())
               .ForMember(dest => dest.Owner, opt => opt.Ignore());
        }
    }
}
