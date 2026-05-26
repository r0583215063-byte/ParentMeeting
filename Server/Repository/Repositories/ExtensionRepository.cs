
using Microsoft.Extensions.DependencyInjection;
using Repository;
using Repository.Entities;
using Repository.Interfaces;
using Repository.Repositories;


namespace SchoolParentMeetingSystem.Repository.Repositories
{
    public static class ExtensionRepository
    {
        //Which class to create when requesting a specific Repository
        public static IServiceCollection AddRepository(this IServiceCollection services)
        {
            services.AddScoped<IRepository<Parent>, ParentRepository>();
            services.AddScoped<IRepository<ParentMeeting>, ParentMeetingRepository>();
            services.AddScoped<IRepository<ParentAvailability>, ParentAvailabilityRepository>();
            services.AddScoped<IRepository<Teacher>, TeacherRepository>();
            services.AddScoped<IRepository<School>, SchoolRepository>();
            services.AddScoped<IRepository<Student>, StudentRepository>();
            return services;
        }
    }
}
