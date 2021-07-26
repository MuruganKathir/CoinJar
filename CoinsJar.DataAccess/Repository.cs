namespace CoinJar.DataAccess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using System.Linq.Expressions;
    using System.Linq;
    using CoinJar.Interfaces;

    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DbContext dbContext;
        internal DbSet<T> dbSet;

        public Repository(DbContext context)
        {
            dbContext = context;
            this.dbSet = context.Set<T>();
        }

        public void Add(T entity)
        {
            // insert into Account
            this.dbSet.Add(entity);
        }

        public void AddRange(IEnumerable<T> entities)
        {
            this.dbSet.AddRange(entities);
        }


        public IEnumerable<T> GetAll(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            //include properties will be separated
            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }
            return query.ToList();
        }
        public IEnumerable<T> GetAll()
        {
            return this.dbSet.ToList();
        }
        public T Get(int id)
        {
            return this.dbSet.Find(id);
        }

        public void Remove(int id)
        {
            T entity = this.dbSet.Find(id);
            this.dbSet.Remove(entity);
        }

        public void Remove(T entity)
        {
            this.dbSet.Remove(entity);
        }

        public void RemoveAll(IEnumerable<T> entities)
        {
            this.dbSet.RemoveRange(entities);
        }
    }
}
