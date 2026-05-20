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
    internal class TeacherRepository : IRepository<Teacher>
    {
        private readonly IContext ctx;

        public TeacherRepository(IContext context)
        {
            ctx = context;
        }

        public async Task<List<Teacher>> GetAsync(Expression<Func<Teacher, bool>> predicate)
        {
            return await ctx.Teachers.Where(predicate).ToListAsync();
        }

        public async Task<Teacher> AddItem(Teacher item)
        {
            ctx.Teachers.Add(item);
            await ctx.Save();
            return item;
        }

        public async Task DeleteItem(int id)
        {
            var teacher = await ctx.Teachers.FindAsync(id);
            if (teacher == null)
                return;

            ctx.Teachers.Remove(teacher);
            await ctx.Save();
        }

        public async Task<List<Teacher>> GetAll()
        {
            return await ctx.Teachers.ToListAsync();
        }

        public async Task<Teacher> GetById(int id)
        {
            return await ctx.Teachers.FindAsync(id);
        }

        public async Task<Teacher> UpdateItem(int id, Teacher item)
        {
            var existingTeacher = await ctx.Teachers.FindAsync(id);

            if (existingTeacher == null)
                return null;

            existingTeacher.FullName = item.FullName;
            existingTeacher.ClassName = item.ClassName;
            existingTeacher.SchoolId = item.SchoolId;

            await ctx.Save();
            return existingTeacher;
        }
    }
}