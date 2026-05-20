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
    internal class ParentRepository : IRepository<Parent>
    {
        private readonly IContext ctx;

        public ParentRepository(IContext context)
        {
            ctx = context;
        }

        public async Task<List<Parent>> GetAsync(Expression<Func<Parent, bool>> predicate)
        {
            return await ctx.Parents.Where(predicate).ToListAsync();
        }

        public async Task<Parent> AddItem(Parent item)
        {
            ctx.Parents.Add(item);
            await ctx.Save();
            return item;
        }

        public async Task DeleteItem(int id)
        {
            var parent = await ctx.Parents.FindAsync(id);
            if (parent == null)
                return;

            ctx.Parents.Remove(parent);
            await ctx.Save();
        }

        public async Task<List<Parent>> GetAll()
        {
            return await ctx.Parents.ToListAsync();
        }

        public async Task<Parent> GetById(int id)
        {
            return await ctx.Parents.FindAsync(id);
        }

        public async Task<Parent> UpdateItem(int id, Parent item)
        {
            var existingParent = await ctx.Parents.FindAsync(id);

            if (existingParent == null)
                return null;

            existingParent.ParentIdentity = item.ParentIdentity;
            existingParent.ParentName = item.ParentName;
            existingParent.ParentEmail = item.ParentEmail;

            await ctx.Save();
            return existingParent;
        }
    }
}