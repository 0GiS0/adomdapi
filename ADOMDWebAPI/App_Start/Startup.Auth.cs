using Microsoft.Owin.Security.ActiveDirectory;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ADOMDWebAPI
{
    public partial class Startup
    {

        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseWindowsAzureActiveDirectoryBearerAuthentication(
                new WindowsAzureActiveDirectoryBearerAuthenticationOptions
                {                    
                    Audience = ConfigurationManager.AppSettings["aad:clientId"],
                    Tenant = ConfigurationManager.AppSettings["aad:domain"]
                });
        }

    }
}