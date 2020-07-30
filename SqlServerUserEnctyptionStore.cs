#if SQLServer
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Reflection;

namespace SecureUserAccess
{
    static class Ext
    {
        public static IQueryable<TSource> Search<TSource, TValue>(this IQueryable<TSource> source, Expression<Func<TSource, TValue>> selector, TValue value)
        {
            var predicate = Expression.Lambda<Func<TSource, bool>>(
                Expression.Equal(
                    selector.Body,
                    Expression.Constant(value, typeof(TValue))
                ), selector.Parameters);
            return source.Where(predicate);
        }

        static PropertyInfo GetPropertyInfo<TSource, TValue>(this Expression<Func<TSource, TValue>> @propSelector)
        {
            Type type = typeof(TSource);

            MemberExpression member = @propSelector.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    @propSelector.ToString()));

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    @propSelector.ToString()));

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a property that is not from type {1}.",
                    @propSelector.ToString(),
                    type));

            return propInfo;

        }

        public static void SetValue<TSource, TValue>(this Expression<Func<TSource, TValue>> @propSelector, TSource @object, TValue value)
        {
            var prop = propSelector.GetPropertyInfo();
            prop.SetValue(@object, value);
        }
        public static TValue GetValue<TSource, TValue>(this Expression<Func<TSource, TValue>> @propSelector, TSource @object)
        {
            var prop = propSelector.GetPropertyInfo();
            TValue value = (TValue)prop.GetValue(@object);
            return value;
        }
    }

    public class SqlServerUserEnctyptionStore<TUser> where TUser : class, new()
    {
        private readonly Expression<Func<TUser, string>> passwordSelector;
        private readonly Expression<Func<TUser, string>> usernameSelector;

        public SqlServerUserEnctyptionStore(Expression<Func<TUser, string>> usernameSelector, Expression<Func<TUser, string>> passwordSelector)
        {
            this.usernameSelector = usernameSelector;
            this.passwordSelector = passwordSelector;
        }

        public TUser ValidateUser(DbContext context, string username, string password)
        {
            var user = context.Set<TUser>().Search<TUser, string>(usernameSelector, username).FirstOrDefault();
            if (user == null) return null;

            if (!PasswordHash.Validate(password, passwordSelector.GetValue(user))) return null;
            return user;
        }

        public void SetPassword(TUser user, string password)
        {
            passwordSelector.SetValue(user, password);
        }
    }
}
#endif