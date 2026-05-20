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
    internal class StudentRepository : IRepository<Student>
    {
        private readonly IContext ctx;

        public StudentRepository(IContext context)
        {
            ctx = context;
        }

        public async Task<List<Student>> GetAsync(Expression<Func<Student, bool>> predicate)
        {
            return await ctx.Students.Where(predicate).ToListAsync();
        }

        public async Task<Student> AddItem(Student item)
        {
            ctx.Students.Add(item);
            await ctx.Save();
            return item;
        }

        public async Task DeleteItem(int id)
        {
            var student = await ctx.Students.FindAsync(id);
            if (student == null)
                return;

            ctx.Students.Remove(student);
            await ctx.Save();
        }

        public async Task<List<Student>> GetAll()
        {
            return await ctx.Students.ToListAsync();
        }

        public async Task<Student> GetById(int id)
        {
            return await ctx.Students.FindAsync(id);
        }

        public async Task<Student> UpdateItem(int id, Student item)
        {
            var existingStudent = await ctx.Students.FindAsync(id);

            if (existingStudent == null)
                return null;

            existingStudent.StudentIdentity = item.StudentIdentity;
            existingStudent.FirstName = item.FirstName;
            existingStudent.LastName = item.LastName;
            existingStudent.ClassName = item.ClassName;
            existingStudent.ParentId = item.ParentId;
            existingStudent.SchoolId = item.SchoolId;

            await ctx.Save();
            return existingStudent;
        }
    }
}