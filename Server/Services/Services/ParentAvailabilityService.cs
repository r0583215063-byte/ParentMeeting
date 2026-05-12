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
    // שים לב: הורדתי את ה-internal כדי למנוע בעיות גישה ב-Controller
    public class ParentAvailabilityService : IService<ParentAvailabilityDto>
    {
        private readonly IRepository<ParentAvailability> repository;
        private readonly IMapper mapper;

        public ParentAvailabilityService(IRepository<ParentAvailability> repository, IMapper map)
        {
            this.repository = repository;
            this.mapper = map;
        }

        // מתודה אחת ומאוחדת לשליפה לפי בית ספר עם ניקוי אוטומטי
        public async Task<List<ParentAvailabilityDto>> GetBySchoolId(int schoolId)
        {
            // שליפת כל הנתונים (מכיוון שה-Repository הוא גנרי)
            var allEntities = await repository.GetAll();

            // סינון ראשוני לפי בית ספר
            var schoolEntities = allEntities.Where(t => t.SchoolId == schoolId).ToList();

            var today = DateTime.Today;

            // 1. איתור ומחיקת אילוצים ישנים
            var expiredEntities = schoolEntities
                .Where(e => e.MeetingDate.Date < today)
                .ToList();

            if (expiredEntities.Any())
            {
                foreach (var expired in expiredEntities)
                {
                    await repository.DeleteItem(expired.Id);
                }

                // 2. השארת רק הפגישות הרלוונטיות להחזרה למשתמש
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