using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BCrypt.Net;

namespace AISDisciplineDesc
{
    internal class SupabaseClient
    {
        private readonly RestClient client;
        private const string BaseURL = "https://mczpqiixbyvxxsiectok.supabase.co";
        private readonly string APIkey;

        public SupabaseClient()
        {
            client = new RestClient(BaseURL); //Инициализация клиента RestRequest
            APIkey = AppSettings.ApiKey;
        }

        private RestRequest CreateRequest(string endpoint, Method method = Method.Post) //Структура запроса к Supabase
        {
            var request = new RestRequest(endpoint, method);
            request.AddHeader("apikey", APIkey);
            request.AddHeader("MainWindow", $"Bearer {APIkey}");
            request.AddHeader("Content-Type", "application/json");
            return request;
        }

        public async Task<bool> AuthenticateUser(string login, string password) //Авторизация
        {
            try
            {
                var request = CreateRequest("rest/v1/rpc/auth_user", Method.Post);
                request.AddJsonBody(new { login }); 
                var response = await client.ExecuteAsync(request);

                if (!response.IsSuccessful || string.IsNullOrEmpty(response.Content))
                    return false;

                var result = JObject.Parse(response.Content);
                if (result["id"] == null || result["id"].Type == JTokenType.Null)
                    return false;

                // Получаем хэш пароля из результата
                string hashedPassword = result["password_hash"]?.Value<string>();
                if (string.IsNullOrEmpty(hashedPassword))
                    return false;

                // Проверяем введённый пароль с хэшем из БД
                bool isValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
                if (!isValid)
                    return false;

                AppState.CurrentUser = new UserData
                {
                    id = result["id"]?.Value<int>() ?? 0,
                    login = login,
                    name = result["name"]?.Value<string>(),
                    email = result["email"]?.Value<string>(),
                    role = result["role"]?.Value<string>(),
                    phone = result["phone"]?.Value<string>(),
                    division = result["division"]?.Value<string>(),
                    address = result["address"]?.Value<string>(),
                    unit = result["unit"]?.Value<string>()
                };
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> DocsInformation() //Метод необходимый для заполнения datagrid
        {
            var request = CreateRequest("/rest/v1/rpc/info_documents_for_lead");
            var response = await client.ExecuteAsync(request);
            try
            {
                var info = JsonConvert.DeserializeObject<List<Documents>>(response.Content);
                AppState.Documentation = info;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> AdminInformation() //Метод необходимый для заполнения datagrid
        {
            var request = CreateRequest("/rest/v1/rpc/all_data_users");
            var response = await client.ExecuteAsync(request);
            try
            {
                var info = JsonConvert.DeserializeObject<List<AdminData>>(response.Content);
                AppState.AdminDataUsers = info;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> CreateUser(string clogin, string cpassword, string cname, string cdivision, string cunit, string crole)
        {

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(cpassword);
            var param = new { clogin, cpassword = hashedPassword, cname, cdivision, cunit, crole };
            var request = CreateRequest("rest/v1/rpc/create_users", Method.Post);
            request.AddJsonBody(param);
            var response = await client.ExecuteAsync(request);

            return response.IsSuccessful && response.Content == "true";
        }

        public async Task<bool> DeleteUser(int userId)
        {
            var param = new { user_id = userId };
            var request = CreateRequest("rest/v1/rpc/delete_user", Method.Post);
            request.AddJsonBody(param);
            var response = await client.ExecuteAsync(request);
            return response.IsSuccessful && response.Content == "true";
        }

        public async Task<bool> DivInformation() //Метод необходимый для заполнения combobox подразделений
        {
            var request = CreateRequest("/rest/v1/rpc/divisions_for_combobox");
            var response = await client.ExecuteAsync(request);
            try
            {
                var info = JsonConvert.DeserializeObject<List<Divisions>>(response.Content);
                AppState.divisions = info;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UnitInformation() //Метод необходимый для заполнения combobox подразделений
        {
            var request = CreateRequest("/rest/v1/rpc/units_for_combobox");
            var response = await client.ExecuteAsync(request);
            try
            {
                var info = JsonConvert.DeserializeObject<List<Units>>(response.Content);
                AppState.units = info;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> CreateOrder(string cunit, string cdivision, string cdescription, string cname, DateTime cduedate, DateTime cdatedispatch)
        {
            var param = new { cunit, cdivision, cdescription, cname, cduedate, cdatedispatch };
            var request = CreateRequest("rest/v1/rpc/create_order", Method.Post);
            request.AddJsonBody(param);
            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful || response.Content != "true")
            {
                string error = $"Status: {response.StatusCode}, Content: {response.Content}, Error: {response.ErrorMessage}";
                //MessageBox.Show(error);
            }

            return response.StatusCode == System.Net.HttpStatusCode.OK && response.Content == "true";
        }

        public async Task<bool> UpdateStatusOrder(int upid, string upstatus) //Метод обновления данных о статусе задач сотрудников
        {
            var param = new { upid, upstatus };
            var request = CreateRequest("rest/v1/rpc/update_document_status", Method.Post);
            request.AddJsonBody(param);
            var response = await client.ExecuteAsync(request);
            return response.StatusCode == System.Net.HttpStatusCode.NoContent;
        }

        public async Task<bool> UpdateUser(int id, string login, string password, string name, string division, string unit, string role)
        {
            try
            {
                var request = CreateRequest("rest/v1/rpc/update_user", Method.Post);
                request.AddJsonBody(new
                {
                    p_id = id,
                    p_login = login,
                    p_password = password, 
                    p_name = name,
                    p_division = division,
                    p_unit = unit,
                    p_role = role
                });
                var response = await client.ExecuteAsync(request);

                return response.IsSuccessful && response.Content?.ToLower() == "true";
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
