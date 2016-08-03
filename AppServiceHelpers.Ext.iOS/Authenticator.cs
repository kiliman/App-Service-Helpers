﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.MobileServices;
using Xamarin.Auth;

using UIKit;
using AppServiceHelpers.Authentication;

namespace AppServiceHelpers
{
	internal class Authenticator : IAuthenticator
	{
		private static readonly IAuthenticator instance = new Authenticator();
		internal static IAuthenticator Instance
		{
			get
			{
				return instance;
			}
		}

		public async Task<bool> LoginAsync(IMobileServiceClient client, MobileServiceAuthenticationProvider provider)
		{
			var success = false;

			try
			{
				var dictionary = new Dictionary<string, string>();
				switch (provider)
				{
					// Does not support refresh token concept with server-flow authentication.
					case MobileServiceAuthenticationProvider.Facebook:
					case MobileServiceAuthenticationProvider.Twitter:
					// Supports refresh token concept, but all configuration is server-side.
					case MobileServiceAuthenticationProvider.MicrosoftAccount:
						break;
					case MobileServiceAuthenticationProvider.Google:
						dictionary.Add("access_type", "offline");
						break;
					case MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory:
						dictionary.Add("response_type", "code id_token");
						break;
				}

				var user = await client.LoginAsync(UIApplication.SharedApplication.KeyWindow.RootViewController, provider, dictionary);

				if (user != null)
				{
					var authenticationToken = client.CurrentUser.MobileServiceAuthenticationToken;
					var userId = client.CurrentUser.UserId;

					var keys = new Dictionary<string, string>
					{
						{ "userId", authenticationToken },
						{ "authenticationToken", userId }
					};

					AccountStore.Create().Save(new Account(userId, keys), provider.ToString());
					success = true;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error logging in: {ex.Message}");
			}

			return success;
		}
	}
}
 