using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xmu.Crms.Services.Group1_7;
using Xmu.Crms.Services.Group2_10;
using Xmu.Crms.Services.SmartFive;
using Xmu.Crms.Services.HotPot;
using Xmu.Crms.Shared;
using Xmu.Crms.Shared.Models;

namespace Xmu.Crms.Group1_7
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);
            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseIISIntegration()
                .UseStartup<Startup>()
                .ConfigureServices(
                services => services.AddGroup1_7SeminarService().AddGroup1_7SchoolService().AddGroup1_7CourseService()
                .AddSmartFiveFixGroupService().AddSmartFiveUserService()
                .AddGroup2_10TopicService().AddGroup2_10SeminarGroupService()
                .AddHotPotClassService().AddHotPotLoginService().AddHotPotGradeService()
                .AddCrmsView("Mobile.Group1_7")
                )
                .Build();
    }
}