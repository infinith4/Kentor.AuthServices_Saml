﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kentor.AuthServices;
using Kentor.AuthServices.Configuration;
using Kentor.AuthServices.WebSso;
using Kentor.AuthServices.Metadata;
using System.Globalization;
using System.IdentityModel.Metadata;
using System.IdentityModel.Selectors;
using System.Security.Claims;
using Kentor.AuthServices.Owin;
using Microsoft.Owin.Security.DataProtection;
using System.Security.Cryptography.X509Certificates;
using System.Web.Hosting;
using Kentor.AuthServices_Saml.Models;
using Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.AspNet.Identity.Owin;
[assembly: OwinStartupAttribute(typeof(Kentor.AuthServices_Saml.Startup))]
namespace Kentor.AuthServices_Saml
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //var options = StubFactory.CreateOptions();
            var spOptions = CreateSPOptions();
            //var subject = new Federation(
            //    "http://localhost:13428/federationMetadata",
            //    false,
            //    options);
            //var spOptions = CreateSPOptions();
            var authServicesOptions = new KentorAuthServicesAuthenticationOptions(false)
            {
                SPOptions = spOptions
            };

            var idp = new IdentityProvider(new EntityId("http://stubidp.kentor.se/Metadata"), spOptions)
            {
                AllowUnsolicitedAuthnResponse = true,
                Binding = Saml2BindingType.HttpRedirect,
                SingleSignOnServiceUrl = new Uri("http://stubidp.kentor.se")
            };

            idp.SigningKeys.AddConfiguredKey(
                new X509Certificate2(
                    HostingEnvironment.MapPath(
                        "~/App_Data/Kentor.AuthServices.StubIdp.cer")));

            authServicesOptions.IdentityProviders.Add(idp);

            //// It's enough to just create the federation and associate it
            //// with the options. The federation will load the metadata and
            //// update the options with any identity providers found.
            //var fed = new Federation("http://localhost:52071/Federation", true, authServicesOptions);
            var fed = new Federation("~/Metadata/SambiMetadata.xml", true, authServicesOptions);
            
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            //app.UseKentorAuthServicesAuthentication(CreateAuthServicesOptions());
            app.UseKentorAuthServicesAuthentication(authServicesOptions);

            var context = HttpContext.GetOwinContext();
            context.Environment.Add("KentorAuthServices.idp", new EntityId(YOUR_IDP_ENTITY_ID));

        }
        private static SPOptions CreateSPOptions()
        {
            var swedish = CultureInfo.GetCultureInfo("sv-se");

            var organization = new Organization();
            organization.Names.Add(new LocalizedName("Kentor", swedish));
            organization.DisplayNames.Add(new LocalizedName("Kentor IT AB", swedish));
            organization.Urls.Add(new LocalizedUri(new Uri("http://www.kentor.se"), swedish));

            var spOptions = new SPOptions
            {
                EntityId = new EntityId("http://localhost:57294/AuthServices"),
                ReturnUrl = new Uri("http://localhost:57294/Account/ExternalLoginCallback"),
                DiscoveryServiceUrl = new Uri("http://localhost:52071/DiscoveryService"),
                Organization = organization
            };

            var techContact = new ContactPerson
            {
                Type = ContactType.Technical
            };
            techContact.EmailAddresses.Add("authservices@example.com");
            spOptions.Contacts.Add(techContact);

            var supportContact = new ContactPerson
            {
                Type = ContactType.Support
            };
            supportContact.EmailAddresses.Add("support@example.com");
            spOptions.Contacts.Add(supportContact);

            var attributeConsumingService = new AttributeConsumingService("AuthServices")
            {
                IsDefault = true,
            };

            attributeConsumingService.RequestedAttributes.Add(
                new RequestedAttribute("urn:someName")
                {
                    FriendlyName = "Some Name",
                    IsRequired = true,
                    NameFormat = RequestedAttribute.AttributeNameFormatUri
                });

            attributeConsumingService.RequestedAttributes.Add(
                new RequestedAttribute("Minimal"));

            spOptions.AttributeConsumingServices.Add(attributeConsumingService);

            spOptions.ServiceCertificates.Add(new X509Certificate2(
                AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "/App_Data/Kentor.AuthServices.Tests.pfx"));

            return spOptions;
        }
    }
}
