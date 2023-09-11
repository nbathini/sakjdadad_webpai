using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PorscheDataAccess.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        IEnumerable<T> GetAll(params Expression<Func<T, object>>[] includeProperties);
        T Find(Expression<Func<T, bool>> match);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> FirstOrDefault(Expression<Func<T, bool>> predicate);
        Task<T> FirstOrDefaultDesc(Expression<Func<T, object>> predicate);
        Task<bool> Any(Expression<Func<T, bool>> predicate);
        void Add(T entity);
        void Update(T entity);
        void Remove(object id);
        Task<int> RemoveWhere(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> GetWhere(Expression<Func<T, bool>> predicate);
        Task<int> CountAll();
        Task<int> CountWhere(Expression<Func<T, bool>> predicate);
        Task<List<T>> ExecuteGetProcedure(List<SqlParameter> sqlParams, string storedProcedure);
        Task<object> ExecuteProcedure(List<SqlParameter> sqlParams, string storedProcedure);
        Task<List<object>> ExecuteProcedure(List<SqlParameter> sqlParams, string storedProcedure, int outVariables);
    }
}
