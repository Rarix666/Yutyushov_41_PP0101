using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISDisciplineDesc.Models
{
    internal class AppSettings
    {
        public static IConfiguration Configuration { get; private set; }

        static AppSettings()
        {
            var builder = new ConfigurationBuilder()
                .AddUserSecrets<MainWindow>(); 
            Configuration = builder.Build();
        }

        public static string ApiKey => Configuration["API_KEY"];
    }
}
