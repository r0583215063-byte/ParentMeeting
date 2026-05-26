using AutoMapper;
using Repository;
using Repository.Entities;
using Repository.Interfaces;
using Service.Dto;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Services
{
    public class SchoolService : IService<SchoolDto>
    {
        private readonly IRepository<School> repository;
        private readonly IRepository<Student> studentRepository;
        private readonly IRepository<ParentMeeting> meetingRepository;
        private readonly IMapper mapper;

        public SchoolService(
            IRepository<School> repository,
            IRepository<Student> studentRepository,
            IRepository<ParentMeeting> meetingRepository,
            IMapper map)
        {
            this.repository = repository;
            this.studentRepository = studentRepository;
            this.meetingRepository = meetingRepository;
            this.mapper = map;
        }

        public async Task<SchoolStatusDto> GetSchoolStatusAsync(int schoolId)
        {
            var students = await studentRepository.GetAsync(s => s.SchoolId == schoolId);
            int studentCount = students != null ? students.Count : 0;

            var meetings = await meetingRepository.GetAsync(m => m.SchoolId == schoolId);
            bool isScheduleGenerated = meetings != null && meetings.Count > 0;

            return new SchoolStatusDto
            {
                StudentCount = studentCount,
                IsScheduleGenerated = isScheduleGenerated
            };
        }

        public async Task SetupMeeting(int schoolId, MeetingSetupDto model)
        {
            var school = await repository.GetById(schoolId);
            if (school == null)
                throw new KeyNotFoundException("בית הספר לא נמצא.");

            school.MeetingDate = model.Date;
            school.MeetingStartTime = model.StartTime;
            school.MeetingEndTime = model.EndTime;
            school.SlotDurationMinutes = model.Duration;

            await repository.UpdateItem(schoolId, school);
        }

        public async Task<List<SchoolDto>> GetBySchoolId(int schoolId)
        {
            var schools = await repository.GetAsync(s => s.Id == schoolId);
            return mapper.Map<List<SchoolDto>>(schools);
        }

        public async Task<SchoolDto> AddItem(SchoolDto item)
        {
            var entity = mapper.Map<School>(item);
            var saved = await repository.AddItem(entity);
            return mapper.Map<SchoolDto>(saved);
        }

        public async Task DeleteItem(int id)
        {
            await repository.DeleteItem(id);
        }

        public async Task<List<SchoolDto>> GetAll()
        {
            var schools = await repository.GetAll();
            return mapper.Map<List<SchoolDto>>(schools);
        }

        public async Task<SchoolDto> GetById(int id)
        {
            var school = await repository.GetById(id);
            return mapper.Map<SchoolDto>(school);
        }

        public async Task<SchoolDto> UpdateItem(int id, SchoolDto item)
        {
            var entity = mapper.Map<School>(item);
            var result = await repository.UpdateItem(id, entity);
            return mapper.Map<SchoolDto>(result);
        }
    }
}