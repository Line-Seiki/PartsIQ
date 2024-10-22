using Newtonsoft.Json;
using PartsIq.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;


namespace PartsIq.Utility
{
    public static class SessionHelper
    {
        public static UserSession GetUserSessionFromCookie()
        {
            // Get the auth cookie
            HttpCookie authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                // Decrypt the auth ticket
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                if (authTicket != null)
                {
                    // Deserialize the user session data from the ticket's UserData
                    return JsonConvert.DeserializeObject<UserSession>(authTicket.UserData);
                }
            }

            // Return null if the cookie is not available or the ticket is invalid
            return null;
        }

        public static UserSession GetUserData(HttpContextBase context)
        {
			try
			{
				HttpCookie authCookie = context.Request.Cookies[FormsAuthentication.FormsCookieName];

				if (authCookie == null) return null;

				var authTicket = FormsAuthentication.Decrypt(authCookie.Value);

				if (authTicket == null) return null;

				var userSession = JsonConvert.DeserializeObject<UserSession>(authTicket.UserData);
				return userSession;
			}
			catch (Exception)
			{

				return null;
			}
        }

		public static UserSession GetUserData(this Controller controller) 
		{
			return GetUserData(controller.HttpContext);
		}
    }
}