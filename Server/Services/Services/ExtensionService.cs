using Microsoft.Extensions.DependencyInjection;
using Repository.Entities;
using SchoolParentMeetingSystem.DataContext;
using SchoolParentMeetingSystem.Repository.Repositories;
using Service.Dto;
using Service.Importing;
using Service.Interfaces;
using Service.Scheduling;
using Service.Services;

namespace SchoolParentMeetingSystem.Service.Services
{
    public static class ExtensionService
    {
        public static IServiceCollection AddServices(this IServiceCollection services, string connectionString)
        {
            services.AddAutoMapper(typeof(MapperProfile));
            services.AddDataLayer(connectionString);
            services.AddRepository();
            services.AddScoped<IRegister<SchoolRegisterDto, School>, SchoolRegisterService>();
            services.AddScoped<ILogin<SchoolLoginDto>, SchoolLoginService>();
            services.AddScoped<IToken<School>, TokenService>();
            services.AddScoped<ExcelImportService>();
            services.AddScoped<IService<SchoolDto>, SchoolService>();
            services.AddScoped<IService<ParentAvailabilityDto>, ParentAvailabilityService>();
            services.AddScoped<IService<ParentMeetingDto>, ParentMeetingService>();
            services.AddScoped<IService<TeacherDto>, TeacherService>();
            services.AddScoped<IService<ParentDto>, ParentService>();
            services.AddScoped<IService<StudentDto>, StudentService>();
            services.AddScoped<ISchedulingService, SchedulingService>();
            return services;
        }
    }
}
