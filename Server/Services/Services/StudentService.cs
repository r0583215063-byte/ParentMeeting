using AutoMapper;
using Repository;
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
    public class StudentService : IService<StudentDto>
    {
        private readonly IRepository<Student> repository;
        private readonly IMapper mapper;

        public StudentService(IRepository<Student> repository, IMapper map)
        {
            this.repository = repository;
            this.mapper = map;
        }

        public async Task<List<StudentDto>> GetBySchoolId(int schoolId)
        {
            var students = await repository.GetAsync(t => t.SchoolId == schoolId);
            return mapper.Map<List<StudentDto>>(students);
        }

        public async Task<StudentDto> AddItem(StudentDto item)
        {
            var entity = mapper.Map<Student>(item);
            var savedEntity = await repository.AddItem(entity);
            return mapper.Map<StudentDto>(savedEntity);
        }

        public async Task DeleteItem(int id)
        {
            await repository.DeleteItem(id);
        }

        public async Task<List<StudentDto>> GetAll()
        {
            var students = await repository.GetAll();
            return mapper.Map<List<StudentDto>>(students);
        }

        public async Task<StudentDto> GetById(int id)
        {
            var student = await repository.GetById(id);
            return mapper.Map<StudentDto>(student);
        }

        public async Task<StudentDto> UpdateItem(int id, StudentDto item)
        {
            var entity = mapper.Map<Student>(item);
            var result = await repository.UpdateItem(id, entity);
            return mapper.Map<StudentDto>(result);
        }
    }
}