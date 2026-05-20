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
    public class SchoolLoginService : ILogin<SchoolLoginDto>
    {
        private readonly IRepository<School> repository;
        private readonly IMapper mapper;
        private readonly IToken<School> _tokenService;

        public SchoolLoginService(IRepository<School> repository, IMapper mapper, IToken<School> token)
        {
            this.repository = repository;
            this.mapper = mapper;
            this._tokenService = token;
        }

        public async Task<SchoolLoginDto> Login(SchoolLoginDto item)
        {
            var schools = await repository.GetAsync(s => s.Name == item.Name);
            var school = schools.FirstOrDefault();

            if (school == null)
                throw new UnauthorizedAccessException("שם משתמש או סיסמה שגויים.");

            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(item.Password, school.Password);

            if (!isPasswordCorrect)
                throw new UnauthorizedAccessException("שם משתמש או סיסמה שגויים.");

            var schoolDto = mapper.Map<SchoolLoginDto>(school);
            schoolDto.Token = _tokenService.GenerateToken(school);

            return schoolDto;
        }
    }
}