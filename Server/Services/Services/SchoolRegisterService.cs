using AutoMapper;
using Repository.Entities;
using Repository.Interfaces;
using Service.Dto;
using Service.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Services
{
    public class SchoolRegisterService : IRegister<SchoolRegisterDto, School>
    {
        private readonly IRepository<School> repository;
        private readonly IMapper mapper;

        public SchoolRegisterService(IRepository<School> repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        public async Task<School> Register(SchoolRegisterDto item)
        {
            if (string.IsNullOrWhiteSpace(item.Password) || item.Password.Length < 6)
                throw new ArgumentException("הסיסמה חייבת להכיל לפחות 6 תווים.");

            var name = item.Name.Trim().ToLower();

            var existingSchools = await repository.GetAsync(s => s.Name.ToLower() == name);
            if (existingSchools.Any())
                throw new InvalidOperationException("שם בית הספר כבר קים במערכת.");

            var entity = mapper.Map<School>(item);
            entity.Name = name;
            entity.Password = BCrypt.Net.BCrypt.HashPassword(item.Password);
            entity.Role = "School";

            return await repository.AddItem(entity);
        }
    }
}