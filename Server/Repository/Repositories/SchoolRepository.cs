using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class SchoolRepository : IRepository<School>
    {
        private readonly IContext ctx;

        public SchoolRepository(IContext context)
        {
            ctx = context;
        }

        public async Task<List<School>> GetAsync(Expression<Func<School, bool>> predicate)
        {
            return await ctx.Schools.Where(predicate).ToListAsync();
        }

        public async Task<School> AddItem(School item)
        {
            ctx.Schools.Add(item);
            await ctx.Save();
            return item;
        }

        public async Task DeleteItem(int id)
        {
            var school = await ctx.Schools.FindAsync(id);
            if (school == null)
                return;

            ctx.Schools.Remove(school);
            await ctx.Save();
        }

        public async Task<List<School>> GetAll()
        {
            return await ctx.Schools.ToListAsync();
        }

        public async Task<School> GetById(int id)
        {
            return await ctx.Schools.FindAsync(id);
        }

        public async Task<School> UpdateItem(int id, School item)
        {
            var existingSchool = await ctx.Schools.FindAsync(id);

            if (existingSchool == null)
                return null;

            existingSchool.Name = item.Name;
            existingSchool.Password = item.Password;
            existingSchool.MeetingDate = item.MeetingDate;
            existingSchool.MeetingStartTime = item.MeetingStartTime;
            existingSchool.MeetingEndTime = item.MeetingEndTime;
            existingSchool.SlotDurationMinutes = item.SlotDurationMinutes;
            existingSchool.Role = item.Role;

            await ctx.Save();
            return existingSchool;
        }
    }
}