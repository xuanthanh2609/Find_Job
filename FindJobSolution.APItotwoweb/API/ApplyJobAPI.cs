﻿using FindJobSolution.ViewModels.Catalog.ApplyJob;
using FindJobSolution.ViewModels.Common;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using FindJobSolution.ViewModels.Catalog.SaveJob;
using System.Net.Http;
using FindJobSolution.ViewModels.Catalog.Jobs;

namespace FindJobSolution.APItotwoweb.API
{
    public interface IApplyJobAPI
    {
        Task<bool> Create(ApplyJobCreateRequest request);

        Task<bool> Delete(int jobinfomationid, int jobseekerid);

        Task<ApplyJobViewModel> GetById(int jobseekerid, int jobinfomationid);

        Task<Tuple<List<ApplyJobViewModel>, List<SaveJobViewModel>>> GetAll();

        Task<List<ApplyJobViewModel>> GetByJobInforId(int id);

        Task<bool> Edit(ApplyJobUpdateRequest request);
    }

    public class ApplyJobAPI : IApplyJobAPI
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplyJobAPI(IHttpClientFactory httpClientFactory, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> Create(ApplyJobCreateRequest request)
        {
            //tạo trang tạo tài khoản mới
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);

            var json = JsonConvert.SerializeObject(request);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            //Hàm lấy api từ backend xử lý đăng ký tài khoản
            var response = await client.PostAsync($"/api/ApplyJob", httpContent);
            //trả về thành công 200 hay thất bại 400 > 500
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> Delete(int jobinfomationid, int jobseekerid)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);

            var response = await client.DeleteAsync($"/api/ApplyJob/{jobinfomationid},{jobseekerid}");
            var body = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<bool>(body);
            return user;
        }

        public async Task<bool> Edit(ApplyJobUpdateRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);

            var json = JsonConvert.SerializeObject(request);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/ApplyJob/JobSeekerId={request.JobSeekerId}/JobInformationId={request.JobInformationId}", httpContent);
            var result = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<bool>(result);
            return JsonConvert.DeserializeObject<bool>(result);
        }

        public async Task<Tuple<List<ApplyJobViewModel>, List<SaveJobViewModel>>> GetAll()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);

            var response = await client.GetAsync($"/api/ApplyJob");
            var body = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<Tuple<List<ApplyJobViewModel>, List<SaveJobViewModel>>>(body);
            return user;
        }

        public async Task<ApplyJobViewModel> GetById(int jobseekerid, int jobinfomationid)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);

            var response = await client.GetAsync($"/api/ApplyJob/Jobseekerid={jobseekerid}/JobInfomationId={jobinfomationid}");
            var body = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<ApplyJobViewModel>(body);
            return user;
        }

        public async Task<List<ApplyJobViewModel>> GetByJobInforId(int id)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);

            var response = await client.GetAsync($"/api/ApplyJob/getjobinfor/{id}");
            var body = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<List<ApplyJobViewModel>>(body);
            return user;
        }

        private async Task<List<T>> GetListAsync<T>(string url, bool requiredLogin = false)
        {
            var sessions = _httpContextAccessor
               .HttpContext
               .Session
               .GetString(SystemConstants.AppSettings.Token);
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration[SystemConstants.AppSettings.BaseAddress]);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);

            var response = await client.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var data = (List<T>)JsonConvert.DeserializeObject(body, typeof(List<T>));
                return data;
            }
            throw new Exception(body);
        }
    }
}