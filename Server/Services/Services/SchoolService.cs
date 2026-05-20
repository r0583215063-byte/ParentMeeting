using AutoMapper;
using Repository.Entities;
using Repository.Interfaces;
using Service.Dto;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Services
{
    public class SchoolService : IService<SchoolDto>
    {
        private readonly IRepository<School> repository;
        private readonly IMapper mapper;

        public SchoolService(IRepository<School> repository, IMapper map)
        {
            this.repository = repository;
            this.mapper = map;
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