using AutoMapper;
using DataContext;
using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;
using Service.Dto;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public class TeacherService : IService<TeacherDto>
    {
        private readonly IRepository<Teacher> repository;
        private readonly IMapper mapper;

        public TeacherService(IRepository<Teacher> repository, IMapper map)
        {
            this.repository = repository;
            this.mapper = map;
        }

        public async Task<List<TeacherDto>> GetBySchoolId(int schoolId)
        {
            var teachers = await repository.GetAsync(t => t.SchoolId == schoolId);
            return mapper.Map<List<TeacherDto>>(teachers);
        }

        public async Task<TeacherDto> AddItem(TeacherDto item)
        {
            var entity = mapper.Map<Teacher>(item);
            var savedTeacher = await repository.AddItem(entity);
            return mapper.Map<TeacherDto>(savedTeacher);
        }

        public async Task DeleteItem(int id)
        {
            var teacher = await repository.GetById(id);
            if (teacher == null)
                throw new KeyNotFoundException("Not found");

            await repository.DeleteItem(id);
        }

        public async Task<List<TeacherDto>> GetAll()
        {
            var teachers = await repository.GetAll();
            return mapper.Map<List<TeacherDto>>(teachers);
        }

        public async Task<TeacherDto> GetById(int id)
        {
            var teacher = await repository.GetById(id);
            return mapper.Map<TeacherDto>(teacher);
        }

        public async Task<TeacherDto> UpdateItem(int id, TeacherDto item)
        {
            var entity = mapper.Map<Teacher>(item);
            var result = await repository.UpdateItem(id, entity);
            return mapper.Map<TeacherDto>(result);
        }
    }
}