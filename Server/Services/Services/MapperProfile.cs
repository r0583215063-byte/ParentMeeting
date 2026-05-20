using AutoMapper;

using Repository;
using Repository.Entities;
using Service.Dto;


namespace SchoolParentMeetingSystem.Service.Services
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {

            CreateMap<School, SchoolRegisterDto>().ReverseMap();
            CreateMap<School, SchoolDto>().ReverseMap();
            CreateMap<School, SchoolLoginDto>().ReverseMap();
            CreateMap<Parent, ParentDto>().ReverseMap();

            CreateMap<ParentMeeting, ParentMeetingDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src =>
                    src.Student != null ? $"{src.Student.FirstName} {src.Student.LastName}" : "לא נמצא"))
                .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src =>
                    src.Parent != null ? src.Parent.ParentName : "לא נמצא"))
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src =>
                    src.Teacher != null ? src.Teacher.FullName : "לא נמצא"))
                .ReverseMap();

            CreateMap<ParentAvailability, ParentAvailabilityDto>()
                .ReverseMap(); CreateMap<Student, StudentDto>().ReverseMap();
            CreateMap<Teacher, TeacherDto>().ReverseMap();
        }
    }
}
