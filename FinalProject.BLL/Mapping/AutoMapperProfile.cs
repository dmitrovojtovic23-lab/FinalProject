using AutoMapper;
using FinalProject.BLL.DTOs;
using FinalProject.DAL;
using TaskStatus = FinalProject.DAL.TaskStatus;
using TaskPriority = FinalProject.DAL.TaskPriority;
using ReminderType = FinalProject.DAL.ReminderType;

namespace FinalProject.BLL.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mappings
            CreateMap<AppUser, UserDto>();
            CreateMap<CreateUserDto, AppUser>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateUserDto, AppUser>()
                .ForAllMembers(opt => opt.Condition((src, dest) => src != null));

            // Task mappings
            CreateMap<TaskItem, TaskDto>()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.TaskTags.Select(tt => tt.Tag).ToList()))
                .ForMember(dest => dest.Reminders, opt => opt.MapFrom(src => src.Reminders.ToList()));
            CreateMap<CreateTaskDto, TaskItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => TaskStatus.Pending))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateTaskDto, TaskItem>()
                .ForAllMembers(opt => opt.Condition((src, dest) => src != null));

            // Category mappings
            CreateMap<TaskCategory, CategoryDto>();
            CreateMap<CreateCategoryDto, TaskCategory>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateCategoryDto, TaskCategory>()
                .ForAllMembers(opt => opt.Condition((src, dest) => src != null));

            // Tag mappings
            CreateMap<TaskTag, TagDto>();
            CreateMap<CreateTagDto, TaskTag>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateTagDto, TaskTag>()
                .ForAllMembers(opt => opt.Condition((src, dest) => src != null));

            // Reminder mappings
            CreateMap<Reminder, ReminderDto>();
            CreateMap<CreateReminderDto, Reminder>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateReminderDto, Reminder>()
                .ForAllMembers(opt => opt.Condition((src, dest) => src != null));
        }
    }
}
