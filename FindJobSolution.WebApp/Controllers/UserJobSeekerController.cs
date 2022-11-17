﻿using Microsoft.AspNetCore.Mvc;
using FindJobSolution.ViewModels.System.UsersJobSeeker;
using FindJobSolution.ViewModels.System.User;
using FindJobSolution.ViewModels.Catalog.ApplyJob;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FindJobSolution.APItotwoweb.API;
using FindJobSolution.ViewModels.System.UsersRecruiter;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using System.Drawing.Printing;
using FindJobSolution.Data.Entities;
using FindJobSolution.ViewModels.Catalog.JobInformations;
using FindJobSolution.ViewModels.Catalog.SaveJob;

namespace FindJobSolution.WebApp.Controllers
{
    public class UserJobSeekerController : Controller
    {
        private readonly IUserAPI _userAPI;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJobSeekerAPI _jobSeekerAPI;
        private readonly IApplyJobAPI _applyJobAPI;
        private readonly ISaveJobAPI _saveJobAPI;

        public UserJobSeekerController(IUserAPI userAPI, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IJobSeekerAPI jobSeekerAPI, IApplyJobAPI applyJobAPI, ISaveJobAPI saveJobAPI)
        {
            _userAPI = userAPI;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _jobSeekerAPI = jobSeekerAPI;
            _applyJobAPI = applyJobAPI;
            _saveJobAPI = saveJobAPI;
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserLoginRequest request)
        {
            if (ModelState.IsValid)
            {
                return View(new UserLoginRequest());
            }

            var token = await _userAPI.Authencate(request);
            //if (token == null)
            //{
            //    ModelState.AddModelError("", token);
            //    return View();
            //}
            var userPrincipal = this.ValidateToken(token);

            IEnumerable<Claim> claims = userPrincipal.Claims;

            var role = claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value;

            if (role.Contains("JobSeeker"))
            {
                var authProperties = new AuthenticationProperties
                {
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                    IsPersistent = false
                };
                HttpContext.Session.SetString("Token", token);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    userPrincipal,
                    authProperties);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View();
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var result = await _jobSeekerAPI.Register(request);
            if (result)
            {
                TempData["result"] = "Create jobseeker successfully";
                return RedirectToAction("Login");
            }
            return View(request);
        }

        private ClaimsPrincipal ValidateToken(string jwtToken)
        {
            IdentityModelEventSource.ShowPII = true;

            SecurityToken validatedToken;
            TokenValidationParameters validationParameters = new TokenValidationParameters();

            validationParameters.ValidateLifetime = true;

            validationParameters.ValidAudience = _configuration["Tokens:Issuer"];
            validationParameters.ValidIssuer = _configuration["Tokens:Issuer"];

            validationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));

            ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(jwtToken, validationParameters, out validatedToken);

            return principal;
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("Token");
            return RedirectToAction("index", "Home");
        }

        public async Task<IActionResult> UserProfile()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var data = await _jobSeekerAPI.GetByUserId(userId);
            return View(data);
        }

        public async Task<IActionResult> UserJob()
        {
            var all = await _applyJobAPI.GetAll();
            return View(all);
        }

        [HttpGet]
        public IActionResult CancelSaveJob(int jobinfoid, int jobseekerid)
        {
            return View(new SaveJobDeleteRequest()
            {
                JobInformationId = jobinfoid,
                JobSeekerId = jobseekerid,
            });
        }

        [HttpPost]
        public async Task<IActionResult> CancelSaveJob(SaveJobDeleteRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var result = await _saveJobAPI.Delete(request.JobSeekerId, request.JobInformationId);
            if (result)
            {
                return RedirectToAction("index");
            }
            return View(request);
        }

        [HttpGet]
        public IActionResult CancelApplyJob(int jobinfoid, int jobseekerid)
        {
            return View(new ApplyJobDeleteRequest()
            {
                JobInformationId = jobinfoid,
                JobSeekerId = jobseekerid,
            });
        }

        [HttpPost]
        public async Task<IActionResult> CancelApplyJob(ApplyJobDeleteRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var result = await _applyJobAPI.Delete(request.JobSeekerId, request.JobInformationId);
            if (result)
            {
                return RedirectToAction("index");
            }
            return View(request);
        }
    }
}