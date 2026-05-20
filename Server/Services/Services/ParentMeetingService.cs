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
    public class ParentMeetingService : IService<ParentMeetingDto>
    {
        private readonly IRepository<ParentMeeting> repository;
        private readonly IMapper mapper;

        public ParentMeetingService(IRepository<ParentMeeting> repository, IMapper map)
        {
            this.repository = repository;
            this.mapper = map;
        }

        public async Task<List<ParentMeetingDto>> GetBySchoolId(int schoolId)
        {
            var meetings = await repository.GetAsync(t => t.SchoolId == schoolId);
            return mapper.Map<List<ParentMeetingDto>>(meetings);
        }

        public async Task<ParentMeetingDto> AddItem(ParentMeetingDto item)
        {
            var entity = mapper.Map<ParentMeeting>(item);
            var saved = await repository.AddItem(entity);
            return mapper.Map<ParentMeetingDto>(saved);
        }

        public async Task<List<ParentMeetingDto>> GetAll()
        {
            var entities = await repository.GetAll();
            return mapper.Map<List<ParentMeetingDto>>(entities);
        }

        public async Task<ParentMeetingDto> GetById(int id)
        {
            var entity = await repository.GetById(id);
            return mapper.Map<ParentMeetingDto>(entity);
        }

        public async Task<ParentMeetingDto> UpdateItem(int id, ParentMeetingDto item)
        {
            var entity = mapper.Map<ParentMeeting>(item);
            var result = await repository.UpdateItem(id, entity);
            return mapper.Map<ParentMeetingDto>(result);
        }

        public async Task DeleteItem(int id)
        {
            await repository.DeleteItem(id);
        }
    }
}