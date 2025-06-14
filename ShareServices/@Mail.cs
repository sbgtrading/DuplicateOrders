// 
// Copyright (C) 2025, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;
#endregion

namespace NinjaTrader.NinjaScript.ShareServices
{
	[TypeConverter("NinjaTrader.NinjaScript.ShareServices.MailTypeConverter")]
	public class Mail : ShareService, IPreconfiguredProvider
	{
		private object icon;

		/// <summary>
		/// This MUST be overridden for any custom service properties to be copied over when instances of the service are created
		/// </summary>
		/// <param name="ninjaScript"></param>
		public override void CopyTo(NinjaScript ninjaScript)
		{
			base.CopyTo(ninjaScript);

			// Recompiling NinjaTrader.Custom after a Share service has been added will cause the Type to change.
			//  Use reflection to set the appropriate properties, rather than casting ninjaScript to Mail.
			PropertyInfo[] props = ninjaScript.GetType().GetProperties();
			foreach (PropertyInfo pi in props)
			{
				if (pi.Name == "FromMailAddress")
					pi.SetValue(ninjaScript, FromMailAddress);
				if (pi.Name == "SenderDisplayName")
					pi.SetValue(ninjaScript, SenderDisplayName);
				else if (pi.Name == "IsBodyHtml")
					pi.SetValue(ninjaScript, IsBodyHtml);
				else if (pi.Name == "Password")
					pi.SetValue(ninjaScript, Password);
				else if (pi.Name == "Port")
					pi.SetValue(ninjaScript, Port);
				else if (pi.Name == "Server")
					pi.SetValue(ninjaScript, Server);
				else if (pi.Name == "Subject")
					pi.SetValue(ninjaScript, Subject);
				else if (pi.Name == "ToMailAddress")
					pi.SetValue(ninjaScript, ToMailAddress);
				else if (pi.Name == "CcMailAddress")
					pi.SetValue(ninjaScript, CcMailAddress);
				else if (pi.Name == "UserName")
					pi.SetValue(ninjaScript, UserName);
				else if (pi.Name == "UseSSL")
					pi.SetValue(ninjaScript, UseSSL);
				else if (pi.Name == "OAuthToken")
					pi.SetValue(ninjaScript, OAuthToken);
				else if (pi.Name == "RefreshToken")
					pi.SetValue(ninjaScript, RefreshToken);
			}
		}

		public override object Icon => icon ??= System.Windows.Application.Current.TryFindResource("ShareIconEmail");

		public override async Task OnAuthorizeAccount()
		{
			Tuple<string, string, string> ret = null;
			if (string.Equals(SelectedPreconfiguredSetting, Custom.Resource.ShareMailPreconfiguredGmail))           // Google OAuth
				ret = await Gui.Tools.GoogleOAuthHelper.Authorize();
			else if (string.Equals(SelectedPreconfiguredSetting, Custom.Resource.ShareMailPreconfiguredOutlook))    // Microsoft OAuth
				ret = await Gui.Tools.MicrosoftOAuthHelper.SignInAndGetAuthCredentialsAsync(alwaysShowLoginWindow: true);

			if (ret is null || string.IsNullOrWhiteSpace(ret.Item1) || string.IsNullOrWhiteSpace(ret.Item2) || string.IsNullOrWhiteSpace(ret.Item3))
			{
				await Task.FromResult(0);
				return;
			}

			FromMailAddress		= ret.Item3;
			OAuthToken			= ret.Item1;
			RefreshToken		= ret.Item2;
			IsConfigured		= true;

			await Task.FromResult(0);
		}

		public override async Task OnShare(string text, string imageFilePath)
		{
			if (IsConfigured && string.Equals(SelectedPreconfiguredSetting, Custom.Resource.ShareMailPreconfiguredGmail))
			{
				// In case Google OAuth fails, try refreshing account access, and if that fails re-request authorization
				if (!await Core.Globals.SendGMail(FromMailAddress, SenderDisplayName, ToMailAddress.Split(',', ';'), CcMailAddress.Split(',', ';'), text, Subject, imageFilePath, OAuthToken))
				{
					OAuthToken = (await Gui.Tools.GoogleOAuthHelper.Authorize(RefreshToken)).Item1;
					if (!await Core.Globals.SendGMail(FromMailAddress, SenderDisplayName, ToMailAddress.Split(',', ';'), CcMailAddress.Split(',', ';'), text, Subject, imageFilePath, OAuthToken))
					{
						await OnAuthorizeAccount();
						if (await Core.Globals.SendGMail(FromMailAddress, SenderDisplayName, ToMailAddress.Split(',', ';'), CcMailAddress.Split(',', ';'), text, Subject, imageFilePath, OAuthToken))
							LogAndPrint(typeof(Custom.Resource), "ShareMailSentSuccessfully", new object[] { Name }, Cbi.LogLevel.Information);
					}
				}
				else
					LogAndPrint(typeof(Custom.Resource), "ShareMailSentSuccessfully", new object[] { Name }, Cbi.LogLevel.Information);
				return;
			}

			if (IsConfigured && string.Equals(SelectedPreconfiguredSetting, Custom.Resource.ShareMailPreconfiguredOutlook))
			{
				if (!await Core.Globals.SendOutlookMailAsync(ToMailAddress.Split(',', ';'), CcMailAddress.Split(',', ';'), text, Subject, imageFilePath, OAuthToken))
				{
					OAuthToken = (await Gui.Tools.MicrosoftOAuthHelper.SignInAndGetAuthCredentialsAsync()).Item1;
					if (await Core.Globals.SendOutlookMailAsync(ToMailAddress.Split(',', ';'), CcMailAddress.Split(',', ';'), text, Subject, imageFilePath, OAuthToken))
						LogAndPrint(typeof(Custom.Resource), "ShareMailSentSuccessfully", new object[] { Name }, Cbi.LogLevel.Information);
				}
				else
					LogAndPrint(typeof(Custom.Resource), "ShareMailSentSuccessfully", new object[] { Name }, Cbi.LogLevel.Information);
				return;
			}

			string mailPassword	= Decrypt(Password);
			string mailUserName = Decrypt(UserName);

			if (Server.Trim().Length == 0 || Port == 0 || mailUserName.Trim().Length == 0 || mailPassword.Trim().Length == 0)
			{
				Cbi.Log.Process(typeof(Resource), "CoreGlobalsSendMail", null, Cbi.LogLevel.Error, Cbi.LogCategories.Default); // Don't do LogLevel.Alert or you'll get a recursive call to email the alert
				return;
			}

			try
			{
				if (await Core.Globals.SendMailToServer(FromMailAddress, DisplayName, ToMailAddress.Split(',', ';'), CcMailAddress.Split(',', ';'), text, Subject, imageFilePath, Server, Port, mailUserName, mailPassword, IsBodyHtml))
					LogAndPrint(typeof(Custom.Resource), "ShareMailSentSuccessfully", new object[] { Name }, Cbi.LogLevel.Information);
			}
			catch (Exception ex)
			{
				Exception innerEx = ex.InnerException;
				string error = ex.Message;
				while (innerEx != null)
				{
					error += " " + innerEx.Message;
					innerEx = innerEx.InnerException;
				}

				Log(string.Format(Custom.Resource.ShareMailException, error), Cbi.LogLevel.Error);
			}
			finally
			{
				Subject			= string.Empty;
				ToMailAddress	= string.Empty;
			}
		}

		public override async Task OnShare(string text, string imageFilePath, object[] args)
		{
			if (args is { Length: > 1 })
			{
				try
				{
					ToMailAddress	= args[0].ToString();
					Subject			= args[1].ToString();
				}
				catch (Exception exp)
				{
					LogAndPrint(typeof(Custom.Resource), "ShareArgsException", new object[] { exp.Message }, Cbi.LogLevel.Error);
					return;
				}
			}

			await OnShare(text, imageFilePath);
		}

		protected override void OnStateChange()
		{			
			if (State == State.SetDefaults)
			{
				CharacterLimit				= int.MaxValue;
				CharactersReservedPerMedia	= int.MaxValue;
				IsConfigured				= true;
				IsDefault					= false;
				IsImageAttachmentSupported	= true;
				Name						= Custom.Resource.MailServiceName;
				Signature					= Custom.Resource.EmailSignature;
				UseOAuth					= false;

				CcMailAddress				= string.Empty;
				FromMailAddress				= string.Empty;
				IsBodyHtml					= false;
				Port						= 25;
				SenderDisplayName			= string.Empty;
				Server						= string.Empty;
				Subject						= string.Empty;
				ToMailAddress				= string.Empty;
				UserName					= string.Empty;

				PreconfiguredNames			= new List<string>
				{
					Custom.Resource.ShareMailPreconfiguredManual,
					Custom.Resource.ShareMailPreconfiguredAol,
					Custom.Resource.ShareMailPreconfiguredComcast,
					Custom.Resource.ShareMailPreconfiguredGmail,
					Custom.Resource.ShareMailPreconfiguredICloud,
					Custom.Resource.ShareMailPreconfiguredOutlook,
					Custom.Resource.ShareMailPreconfiguredYahoo
				};

				SelectedPreconfiguredSetting = PreconfiguredNames[0];
			}
			else if (State == State.Terminated)
			{
				CcMailAddress	= string.Empty;
				Subject			= string.Empty;
				ToMailAddress	= string.Empty;
			}
		}

		#region IPreconfiguredProvider
		public void ApplyPreconfiguredSettings(string name)
		{
			if (name == Custom.Resource.ShareMailPreconfiguredAol)
			{
				Port			= 587;
				Server			= "smtp.aol.com";
				UseSSL			= true;
				UseOAuth		= false;
				IsConfigured	= true;
			}
			else if (name == Custom.Resource.ShareMailPreconfiguredComcast)
			{
				Port			= 587;
				Server			= "smtp.comcast.net";
				UseSSL			= true;
				UseOAuth		= false;
				IsConfigured	= true;
			}
			else if (name == Custom.Resource.ShareMailPreconfiguredGmail)
			{
				UseOAuth		= true;
				IsConfigured	= false;
			}
			else if (name == Custom.Resource.ShareMailPreconfiguredICloud)
			{
				Port			= 587;
				Server			= "smtp.mail.me.com";
				UseSSL			= true;
				UseOAuth		= false;
				IsConfigured	= true;
			}
			else if (name == Custom.Resource.ShareMailPreconfiguredOutlook)
			{
				UseOAuth		= true;
				IsConfigured	= false;
			}
			else if (name == Custom.Resource.ShareMailPreconfiguredYahoo)
			{
				Port			= 587;
				Server			= "smtp.mail.yahoo.com";
				UseSSL			= true;
				UseOAuth		= false;
				IsConfigured	= true;
			}
			else
			{
				UseOAuth		= false;
				IsConfigured	= true;
			}
		}

		[XmlIgnore]
		public List<string> PreconfiguredNames { get; set; }

		public string SelectedPreconfiguredSetting { get; set; }
		#endregion

		#region Properties
		[Display(ResourceType = typeof(Custom.Resource), Name = "MailServiceMailAddress", GroupName = "ShareServiceParameters", Order = 40)]
		[Required]
		public string FromMailAddress { get; set; }

		[Display(ResourceType = typeof(Custom.Resource), Name = "MailServiceSenderDisplayName", GroupName = "ShareServiceParameters", Order = 45)]        //The name will show up in the text label on the Share window and the Description will be the tooltip. Order determines the order fields show up in the window.
		public string SenderDisplayName { get; set; }

		[Browsable(false)]
		public bool IsBodyHtml { get; set; }

		[Gui.Encrypt]
		[Browsable(false)]
		public string OAuthToken { get; set; }

		[Gui.Encrypt]
		[PasswordPropertyText(true)]
		[Required]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ShareServicePassword", GroupName = "ShareServiceParameters", Order = 60)]
		public string Password { get; set; }

		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MailServicePort", GroupName = "ShareServiceParameters", Order = 20)]
		[Required]
		public int Port { get; set; }

		[Gui.Encrypt]
		[Browsable(false)]
		public string RefreshToken { get; set; }

		[Display(ResourceType = typeof(Custom.Resource), Name = "MailServiceServer", GroupName = "ShareServiceParameters", Order = 10)]
		[Required]
		public string Server { get; set; }

		[ShareField]																													//This indicates this property should show up in the Share window
		[Display(ResourceType = typeof(Custom.Resource), Name = "MailSubject", Description = "MailSubjectDescription", Order = 100)]		//The name will show up in the text label on the Share window and the Description will be the tooltip. Order determines the order fields show up in the window.
		[Browsable(false)]
		[XmlIgnore]
		public string Subject { get; set; }

		[ShareField]																													//This indicates this property should show up in the Share window
		[Display(ResourceType = typeof(Custom.Resource), Name = "MailToAddress", Description = "MailToAddressDescription", Order = 0)]	//The name will show up in the text label on the Share window and the Description will be the tooltip. Order determines the order fields show up in the window.
		[Browsable(false)]
		[XmlIgnore]
		public string ToMailAddress { get; set; }

		[ShareField]																													//This indicates this property should show up in the Share window
		[Display(ResourceType = typeof(Custom.Resource), Name = "MailCcAddress", Description = "MailCcAddressDescription", Order = 1)]	//The name will show up in the text label on the Share window and the Description will be the tooltip. Order determines the order fields show up in the window.
		[Browsable(false)]
		[XmlIgnore]
		public string CcMailAddress { get; set; }

		[Gui.Encrypt]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ShareServiceUserName", GroupName = "ShareServiceParameters", Order = 50)]
		[Required]
		public string UserName { get; set; }

		[Gui.Encrypt]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MailServiceSSL", GroupName = "ShareServiceParameters", Order = 30)]
		public bool UseSSL { get; set; }
		#endregion
	}

	public class MailTypeConverter : TypeConverter
	{
		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attrs)
		{
			Mail mail = component as Mail;
			PropertyDescriptorCollection filtered = new(null);
			TypeConverter converter = TypeDescriptor.GetConverter(typeof(ShareService));
			if (mail?.UseOAuth != true)
				return converter.GetProperties(context, component, attrs);
			PropertyDescriptorCollection props = converter.GetProperties(context, component, attrs);
			if (props != null)
				foreach (PropertyDescriptor property in props)
					if (property.Name is "Password" or "Port" or "Server" or "UseSSL" or "UserName" or "FromMailAddress" ||
						(property.Name is "SenderDisplayName" && string.Equals(mail?.SelectedPreconfiguredSetting, Custom.Resource.ShareMailPreconfiguredOutlook)))
					{
					}
					else
						filtered.Add(property);

			return filtered;
		}

		public override bool GetPropertiesSupported(ITypeDescriptorContext context) => true;
	}
}
