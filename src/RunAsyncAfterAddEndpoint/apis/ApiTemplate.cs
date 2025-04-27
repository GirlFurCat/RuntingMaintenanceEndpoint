using RunAsyncAfterAddEndpoint.database;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RunAsyncAfterAddEndpoint.apis
{
    public class ApiTemplate(DapperHelper dapper)
    {
        public async Task<ActionResult> GetAsync<T>(T Entity, string sql)
        {
            return new JsonResult(await dapper.GetAsync(sql, Entity));
        }

        public async Task<ActionResult> GetPagesAsync<T>(T Entity, string sql)
        {
            return new JsonResult(await dapper.GetPageAsync(sql, Entity));
        }

        public async Task<ActionResult> PostAsync<T>(T Entity, string sql)
        {
            return new JsonResult((await dapper.ExecuteAsync(sql, Entity)) > 0 ? true : false);
        }

        public async Task<ActionResult> PutAsync<T>(T Entity, string sql)
        {
            string SQLStr = $"PUT SQL:[{BuildSql(Entity, sql)}]" + "\r\n" + PrintFields(Entity);
            return new JsonResult(await Task.FromResult(SQLStr));
        }

        public async Task<ActionResult> DeleteAsync<T>(T Entity, string sql)
        {
            string SQLStr = $"DELETE SQL:[{BuildSql(Entity, sql)}]" + "\r\n" + PrintFields(Entity);
            return new JsonResult(await Task.FromResult(SQLStr));
        }

        private string BuildSql<T>(T Entity, string sql)
        {
            PropertyInfo[] fields = Entity!.GetType().GetProperties();
            foreach(PropertyInfo field in fields)
            {
                string name = field.Name;
                object? value = field.GetValue(Entity);
                if (value != null)
                {
                    sql = sql.Replace($"@{name}", value.ToString()!);
                }
            }
            return sql;
        }

        private static string PrintFields<T>(T entity)
        {
            List<string> stringlist = new List<string>();
            var type = entity!.GetType();
            var fields = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                var value = field.GetValue(entity);
                stringlist.Add($"Field: {field.Name}, Value: {value}");
            }
            return string.Join(";", stringlist);
        }
    }
}
