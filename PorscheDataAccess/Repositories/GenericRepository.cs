using System;
using System.Collections.Generic;
using PorscheDataAccess.PorscheContext;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PorscheDataAccess.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly PorscheDbContext _context;
        public GenericRepository(PorscheDbContext context)
        {
            _context = context;
        }
        public IEnumerable<T> GetAll(params Expression<Func<T, object>>[] includeProperties)
        {
            return _context.Set<T>().ToList();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public T Find(Expression<Func<T, bool>> match)
        {

            return _context.Set<T>().SingleOrDefault(match);
        }

        public async Task<T> FirstOrDefault(Expression<Func<T, bool>> predicate)
            => await _context.Set<T>().FirstOrDefaultAsync(predicate);

        public async Task<T> FirstOrDefaultDesc(Expression<Func<T, object>> predicate)
            => await _context.Set<T>().OrderByDescending(predicate).FirstOrDefaultAsync();

        public async Task<bool> Any(Expression<Func<T, bool>> predicate)
            => await _context.Set<T>().AnyAsync(predicate);

        public void Add(T entity)
        {
            _context.Set<T>().AddAsync(entity);
        }

        public void Update(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Remove(object id)
        {
            T entity = _context.Set<T>().Find(id);
            _context.Set<T>().Remove(entity);
        }

        public async Task<int> RemoveWhere(Expression<Func<T, bool>> predicate)
        {
            T entity = _context.Set<T>().FirstOrDefault(predicate);
            _context.Set<T>().Remove(entity);
            return await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> GetWhere(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().Where(predicate).ToListAsync();
        }

        public async Task<int> CountAll() => await _context.Set<T>().CountAsync();

        public async Task<int> CountWhere(Expression<Func<T, bool>> predicate)
            => await _context.Set<T>().CountAsync(predicate);

        private static string GetParameters(List<SqlParameter> sqlParams)
        {
            var parameters = string.Empty;
            if (sqlParams != null && sqlParams.Count > 0)
            {
                foreach (var param in sqlParams)
                {
                    if (param.Value is string)
                    {
                        param.Value = "'" + param.Value + "'";
                    }
                    parameters = parameters == string.Empty ? $"{parameters}{param.Value}" :
                        $"{parameters}{','}{param.Value}";

                    parameters = param.Direction == System.Data.ParameterDirection.Output ? $"{parameters}{" OUT"}" : parameters;
                }
            }
            return parameters;
        }

        public async Task<List<T>> ExecuteGetProcedure(List<SqlParameter> sqlParams, string storedProcedure)
        {
            List<T> lstRecords = null;
            if (!string.IsNullOrEmpty(storedProcedure))
            {
                if (sqlParams != null && sqlParams.Count > 0)
                {
                    var parameters = GetParameters(sqlParams);
                    lstRecords = await _context.Set<T>().FromSqlRaw($"Select * from {storedProcedure} ({parameters})").AsNoTracking().ToListAsync();
                }
                else
                {
                    lstRecords = await _context.Set<T>().FromSqlRaw(storedProcedure).AsNoTracking().ToListAsync();
                }
            }
            return lstRecords;
        }

        public async Task<object> ExecuteProcedure(List<SqlParameter> sqlParams, string storedProcedure)
        {
            object result = null;
            if (!string.IsNullOrEmpty(storedProcedure) && sqlParams != null && sqlParams.Count > 0)
            {
                var parameters = GetParameters(sqlParams);
                await _context.Database.ExecuteSqlRawAsync($"{storedProcedure} {parameters}", sqlParams.ToArray());
                result = sqlParams[sqlParams.Count - 1].Value;
            }
            return result;
        }

        public async Task<List<object>> ExecuteProcedure(List<SqlParameter> sqlParams, string storedProcedure, int outVariables)
        {
            List<object> result = new List<object>();
            if (!string.IsNullOrEmpty(storedProcedure) && sqlParams != null && sqlParams.Count > 0)
            {
                var parameters = GetParameters(sqlParams);
                await _context.Database.ExecuteSqlRawAsync($"{storedProcedure} {parameters}", sqlParams.ToArray());
                for (int i = 1; i <= outVariables; i++)
                {
                    result.Add(sqlParams[sqlParams.Count - i].Value);
                }
            }
            return result;
        }

    }
}

