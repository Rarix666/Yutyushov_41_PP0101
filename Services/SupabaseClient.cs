using AISDisciplineDesc.Models;
using BCrypt.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfMessageBox = System.Windows.MessageBox;

namespace AISDisciplineDesc.Services
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

                bool? isLocked = result["is_locked"]?.Value<bool>();
                if (isLocked == true)
                {
                    return false; 
                }

                // Получаем хэш пароля из результата
                string hashedPassword = result["password_hash"]?.Value<string>();
                if (string.IsNullOrEmpty(hashedPassword))
                    return false;

                // Проверяем введённый пароль с хэшем из БД
                bool isValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
                if (!isValid)
                    return false;

                AppState.CurrentUser = result.ToObject<UserData>();
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

        public async Task<List<PersonnelData>> GetPersonnelList()
        {
            var request = CreateRequest("/rest/v1/rpc/get_all_users", Method.Post);
            request.AddJsonBody(new { }); // пустой объект, так как функция не требует параметров
            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful) return new List<PersonnelData>();
            return JsonConvert.DeserializeObject<List<PersonnelData>>(response.Content);
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

        public async Task<bool> DeleteUser(int user_id)
        {
            var param = new { user_id };
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

        public async Task<bool> UnitInformation() 
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

        public async Task<bool> CreateOrder(string cunit, string cdivision, string cdescription, string cname, DateTime cduedate, DateTime cdatedispatch, string cfileUrl = null)
        {
            var param = new { cunit, cdivision, cdescription, cname, cduedate, cdatedispatch, cfile_url = cfileUrl };
            var request = CreateRequest("rest/v1/rpc/create_order", Method.Post);
            request.AddJsonBody(param);
            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful || response.Content != "true")
            {
                string error = $"Status: {response.StatusCode}, Content: {response.Content}, Error: {response.ErrorMessage}";
            }

            return response.StatusCode == System.Net.HttpStatusCode.OK && response.Content == "true";
        }

        public async Task<bool> UpdateStatusOrder(int upid, string upstatus) //Метод обновления данных о статусе приказов
        {
            var param = new { upid, upstatus };
            var request = CreateRequest("rest/v1/rpc/update_document_status", Method.Post);
            request.AddJsonBody(param);
            var response = await client.ExecuteAsync(request);
            return response.StatusCode == System.Net.HttpStatusCode.NoContent;
        }

        public async Task<bool> UpdateUser(int p_id, string p_login, string p_password, string p_name, string p_division, string p_unit, string p_role)
        {
            try
            {
                var request = CreateRequest("rest/v1/rpc/update_user", Method.Post);
                request.AddJsonBody(new
                {
                    p_id,
                    p_login,
                    p_password, 
                    p_name,
                    p_division,
                    p_unit,
                    p_role
                });
                var response = await client.ExecuteAsync(request);

                return response.IsSuccessful && response.Content?.ToLower() == "true";
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserProfile(int p_id, string p_phone, string p_email, string p_address, string p_division)
        {
            var request = CreateRequest("/rest/v1/rpc/update_user_profile", Method.Post);
            request.AddJsonBody(new
            {
                p_id,
                p_phone,
                p_email,
                p_address,
                p_division
            });
            var response = await client.ExecuteAsync(request);
            return response.IsSuccessful && response.Content?.ToLower() == "true";
        }

        public async Task<bool> SetUserLockStatus(int user_id, bool lock_status)
        {
            var request = CreateRequest("rest/v1/rpc/toggle_user_lock", Method.Post);
            request.AddJsonBody(new { user_id, lock_status });
            var response = await client.ExecuteAsync(request);
            return response.IsSuccessful && response.Content?.ToLower() == "true";
        }

        // -------Работа с PDF--------

        public async Task<string> UploadDocumentFile(string localFilePath, string bucketName = "documents")
        {
            try
            {
                var baseUrl = BaseURL.TrimEnd('/');
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(localFilePath)}";
                var url = $"{baseUrl}/storage/v1/object/{bucketName}/{fileName}";

                var fileBytes = await File.ReadAllBytesAsync(localFilePath);

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("apikey", APIkey);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {APIkey}");

                    using (var content = new ByteArrayContent(fileBytes))
                    {
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                        var response = await httpClient.PostAsync(url, content);
                        var responseBody = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                            return $"{baseUrl}/storage/v1/object/public/{bucketName}/{fileName}";

                        WpfMessageBox.Show($"Upload failed: {response.StatusCode}\n{responseBody}", "Ошибка загрузки PDF");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show($"Исключение:\n{ex.Message}");
                return null;
            }
        }
        public async Task<string> UploadDocumentFile(byte[] fileData, string fileName, string bucketName = "documents")
        {
            try
            {
                var baseUrl = BaseURL.TrimEnd('/');
                var url = $"{baseUrl}/storage/v1/object/{bucketName}/{fileName}";

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("apikey", APIkey);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {APIkey}");

                    // Content-Type лучше оставить application/pdf, если файл всё равно PDF (даже зашифрованный)
                    // Supabase не проверяет содержимое, можно оставить application/octet-stream
                    using (var content = new ByteArrayContent(fileData))
                    {
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                        var response = await httpClient.PostAsync(url, content);
                        var responseBody = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                            return $"{baseUrl}/storage/v1/object/public/{bucketName}/{fileName}";

                        WpfMessageBox.Show($"Upload failed: {response.StatusCode}\n{responseBody}", "Ошибка загрузки PDF");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show($"Исключение: {ex.Message}");
                return null;
            }
        }
    }
}
