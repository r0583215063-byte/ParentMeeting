using AutoMapper;
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
    public class ParentAvailabilityService : IService<ParentAvailabilityDto>
    {
        private readonly IRepository<ParentAvailability> repository; 
        private readonly IMapper mapper;

        public ParentAvailabilityService(IRepository<ParentAvailability> repository, IMapper map)
        {
            this.repository = repository;
            this.mapper = map;
        }

        public async Task<List<ParentAvailabilityDto>> GetBySchoolId(int schoolId)
        {
            var schoolEntities = await repository.GetAsync(t => t.SchoolId == schoolId);

            var today = DateTime.Today;

            var expiredEntities = schoolEntities
                .Where(e => e.MeetingDate.Date < today)
                .ToList();

            if (expiredEntities.Any())
            {
                foreach (var expired in expiredEntities)
                {
                    await repository.DeleteItem(expired.Id);
                }

                schoolEntities = schoolEntities.Where(e => e.MeetingDate.Date >= today).ToList();
            }

            return mapper.Map<List<ParentAvailabilityDto>>(schoolEntities);
        }

        public async Task<ParentAvailabilityDto> GetById(int id)
        {
            var entity = await repository.GetById(id);
            return mapper.Map<ParentAvailabilityDto>(entity);
        }

        public async Task<List<ParentAvailabilityDto>> GetAll()
        {
            var entities = await repository.GetAll();
            return mapper.Map<List<ParentAvailabilityDto>>(entities);
        }

        public async Task<ParentAvailabilityDto> AddItem(ParentAvailabilityDto item)
        {
            var entity = mapper.Map<ParentAvailability>(item);
            var savedEntity = await repository.AddItem(entity);
            return mapper.Map<ParentAvailabilityDto>(savedEntity);
        }

        public async Task<ParentAvailabilityDto> UpdateItem(int id, ParentAvailabilityDto item)
        {
            var entity = mapper.Map<ParentAvailability>(item);
            var result = await repository.UpdateItem(id, entity);
            return mapper.Map<ParentAvailabilityDto>(result);
        }

        public async Task DeleteItem(int id)
        {
            await repository.DeleteItem(id);
        }
    }
}