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
    internal class ParentMeetingRepository : IRepository<ParentMeeting>
    {
        private readonly IContext ctx;

        public ParentMeetingRepository(IContext context)
        {
            ctx = context;
        }

        public async Task<ParentMeeting> AddItem(ParentMeeting item)
        {
            ctx.ParentMeetings.Add(item);
            await ctx.Save();
            return item;
        }

        public async Task DeleteItem(int id)
        {
            var parentMeeting = await ctx.ParentMeetings.FindAsync(id);
            if (parentMeeting == null)
                return;

            ctx.ParentMeetings.Remove(parentMeeting);

            await ctx.Save();
        }


        public async Task<List<ParentMeeting>> GetAll()
        {
            return await ctx.ParentMeetings
                .Include(m => m.Student)
                .Include(m => m.Parent)
                .Include(m => m.Teacher)
                .ToListAsync();
        }

        public async Task<ParentMeeting> GetById(int id)
        {
            return await ctx.ParentMeetings.FindAsync(id);
        }

        public async Task<ParentMeeting> UpdateItem(int id, ParentMeeting item)
        {
            var existingMeeting = await ctx.ParentMeetings.FindAsync(id);

            if (existingMeeting == null)
                return null;

            existingMeeting.StudentId = item.StudentId;
            existingMeeting.ParentId = item.ParentId;
            existingMeeting.ClassName = item.ClassName;
            existingMeeting.SchoolId = item.SchoolId;
            existingMeeting.MeetingDate = item.MeetingDate;
            existingMeeting.StartTime = item.StartTime;
            existingMeeting.EndTime = item.EndTime;
            existingMeeting.IsPast = item.IsPast;

            await ctx.Save();

            return existingMeeting;
        }
    }
}
