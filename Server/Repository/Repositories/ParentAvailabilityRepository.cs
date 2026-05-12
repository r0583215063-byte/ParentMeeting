using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    internal class ParentAvailabilityRepository : IRepository<ParentAvailability>
    {
        private readonly IContext ctx;

        public ParentAvailabilityRepository(IContext context)
        {
            ctx = context;
        }

        public async Task<ParentAvailability> AddItem(ParentAvailability item)
        {
            ctx.ParentAvailability.Add(item);
            await ctx.Save();
            return item;
        }

        public async Task DeleteItem(int id)
        {
            var parentAvailability = await ctx.ParentAvailability.FindAsync(id);
            if (parentAvailability == null)
                return;

            ctx.ParentAvailability.Remove(parentAvailability);

            await ctx.Save();
        }

        public async Task<List<ParentAvailability>> GetAll()
        {
            return await ctx.ParentAvailability.ToListAsync();
        }

        public async Task<ParentAvailability> GetById(int id)
        {
            return await ctx.ParentAvailability.FindAsync(id);
        }
        public async Task<List<ParentAvailability>> GetBySchoolId(int schoolId)
        {
            return await ctx.ParentAvailability
                .Where(x => x.SchoolId == schoolId) // וודא שזה שם השדה אצלך
                .ToListAsync();
        }

        public async Task<ParentAvailability> UpdateItem(int id, ParentAvailability item)
        {
            var existingAvailability = await ctx.ParentAvailability.FindAsync(id);

            if (existingAvailability == null)
                return null;

            existingAvailability.SchoolId = item.SchoolId;
            existingAvailability.ParentIdentity = item.ParentIdentity;
            existingAvailability.MeetingDate = item.MeetingDate;
            existingAvailability.StartTime = item.StartTime;
            existingAvailability.EndTime = item.EndTime;
            existingAvailability.IsAvailable = item.IsAvailable;

            await ctx.Save();
            return existingAvailability;
        }
    }
}
