using System;
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

namespace Kentor.AuthServices_Saml.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
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
            var fed = new Federation("http://localhost:52071/Federation", true, authServicesOptions);
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
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

    //class StubFactory
    //{
    //    internal static AuthServicesUrls CreateAuthServicesUrls()
    //    {
    //        return new AuthServicesUrls(new Uri("http://localhost"), "/AuthServices");
    //    }

    //    internal static AuthServicesUrls CreateAuthServicesUrlsPublicOrigin(Uri publicOrigin)
    //    {
    //        return new AuthServicesUrls(publicOrigin, "/AuthServices");
    //    }

    //    internal static SPOptions CreateSPOptions()
    //    {
    //        var org = new Organization();

    //        org.Names.Add(new LocalizedName("Kentor.AuthServices", CultureInfo.InvariantCulture));
    //        org.DisplayNames.Add(new LocalizedName("Kentor AuthServices", CultureInfo.InvariantCulture));
    //        org.Urls.Add(new LocalizedUri(
    //            new Uri("http://github.com/KentorIT/authservices"),
    //            CultureInfo.InvariantCulture));

    //        var options = new SPOptions
    //        {
    //            EntityId = new EntityId("https://github.com/KentorIT/authservices"),
    //            MetadataCacheDuration = new TimeSpan(0, 0, 42),
    //            MetadataValidDuration = TimeSpan.FromDays(24),
    //            WantAssertionsSigned = true,
    //            Organization = org,
    //            DiscoveryServiceUrl = new Uri("https://ds.example.com"),
    //            ReturnUrl = new Uri("https://localhost/returnUrl")
    //        };

    //        options.SystemIdentityModelIdentityConfiguration.ClaimsAuthenticationManager
    //            = new ClaimsAuthenticationManagerStub();
    //        options.SystemIdentityModelIdentityConfiguration.AudienceRestriction.AudienceMode
    //            = AudienceUriMode.Never;

    //        AddContacts(options);
    //        AddAttributeConsumingServices(options);

    //        return options;
    //    }


    //    internal static SPOptions CreateSPOptions(Uri publicOrigin)
    //    {
    //        var org = new Organization();

    //        org.Names.Add(new LocalizedName("Kentor.AuthServices", CultureInfo.InvariantCulture));
    //        org.DisplayNames.Add(new LocalizedName("Kentor AuthServices", CultureInfo.InvariantCulture));
    //        org.Urls.Add(new LocalizedUri(
    //            new Uri("http://github.com/KentorIT/authservices"),
    //            CultureInfo.InvariantCulture));

    //        var options = new SPOptions
    //        {
    //            EntityId = new EntityId("https://github.com/KentorIT/authservices"),
    //            MetadataCacheDuration = new TimeSpan(0, 0, 42),
    //            MetadataValidDuration = TimeSpan.FromDays(24),
    //            WantAssertionsSigned = true,
    //            Organization = org,
    //            DiscoveryServiceUrl = new Uri("https://ds.example.com"),
    //            ReturnUrl = new Uri("https://localhost/returnUrl"),
    //            PublicOrigin = publicOrigin
    //        };

    //        options.SystemIdentityModelIdentityConfiguration.ClaimsAuthenticationManager
    //            = new ClaimsAuthenticationManagerStub();
    //        options.SystemIdentityModelIdentityConfiguration.AudienceRestriction.AudienceMode
    //            = AudienceUriMode.Never;

    //        AddContacts(options);
    //        AddAttributeConsumingServices(options);

    //        return options;
    //    }

    //    private static void AddAttributeConsumingServices(SPOptions options)
    //    {
    //        var a1 = new RequestedAttribute("urn:attributeName")
    //        {
    //            FriendlyName = "friendlyName",
    //            NameFormat = RequestedAttribute.AttributeNameFormatUri,
    //            AttributeValueXsiType = ClaimValueTypes.String,
    //            IsRequired = true
    //        };
    //        a1.Values.Add("value1");
    //        a1.Values.Add("value2");

    //        var a2 = new RequestedAttribute("someName");

    //        var acs = new AttributeConsumingService("attributeServiceName")
    //        {
    //            IsDefault = true
    //        };
    //        acs.RequestedAttributes.Add(a1);
    //        acs.RequestedAttributes.Add(a2);

    //        options.AttributeConsumingServices.Add(acs);
    //    }

    //    private static void AddContacts(SPOptions options)
    //    {
    //        var supportContact = new ContactPerson(ContactType.Support)
    //        {
    //            Company = "Kentor",
    //            GivenName = "Anders",
    //            Surname = "Abel",
    //        };

    //        supportContact.TelephoneNumbers.Add("+46 8 587 650 00");
    //        supportContact.TelephoneNumbers.Add("+46 708 96 50 63");
    //        supportContact.EmailAddresses.Add("info@kentor.se");
    //        supportContact.EmailAddresses.Add("anders.abel@kentor.se");

    //        options.Contacts.Add(supportContact);
    //        options.Contacts.Add(new ContactPerson(ContactType.Technical)); // Deliberately void of info.
    //    }

    //    private static IOptions CreateOptions(Func<SPOptions, IOptions> factory)
    //    {
    //        var options = factory(CreateSPOptions());

    //        KentorAuthServicesSection.Current.IdentityProviders.RegisterIdentityProviders(options);
    //        KentorAuthServicesSection.Current.Federations.RegisterFederations(options);

    //        return options;
    //    }

    //    internal static Options CreateOptions()
    //    {
    //        return (Options)CreateOptions(sp => new Options(sp));
    //    }

    //    private static IOptions CreateOptionsPublicOrigin(Func<SPOptions, IOptions> factory, Uri publicOrigin)
    //    {
    //        var options = factory(CreateSPOptions(publicOrigin));

    //        KentorAuthServicesSection.Current.IdentityProviders.RegisterIdentityProviders(options);
    //        KentorAuthServicesSection.Current.Federations.RegisterFederations(options);

    //        return options;
    //    }
    //    internal static Options CreateOptionsPublicOrigin(Uri publicOrigin)
    //    {
    //        return (Options)CreateOptionsPublicOrigin(sp => new Options(sp), publicOrigin);
    //    }

    //    internal static KentorAuthServicesAuthenticationOptions CreateOwinOptions()
    //    {
    //        return (KentorAuthServicesAuthenticationOptions)CreateOptions(
    //            sp => new KentorAuthServicesAuthenticationOptions(false)
    //            {
    //                SPOptions = sp,
    //                SignInAsAuthenticationType = "AuthType",
    //                //DataProtector = new StubDataProtector()
    //            });
    //    }
    //}

    //class StubDataProtector : IDataProtector
    //{
    //    byte[] IDataProtector.Protect(byte[] userData)
    //    {
    //        return Protect(userData);
    //    }

    //    public static byte[] Protect(byte[] userData)
    //    {
    //        return userData.Select((b, i) =>
    //            i < 6 ? b : (byte)(b ^ 42)
    //            ).ToArray();
    //    }

    //    byte[] IDataProtector.Unprotect(byte[] protectedData)
    //    {
    //        return Unprotect(protectedData);
    //    }

    //    public static byte[] Unprotect(byte[] protectedData)
    //    {
    //        return protectedData.Select((b, i) =>
    //            i < 6 ? b : (byte)(b ^ 42)
    //            ).ToArray();
    //    }
    //}

    //class ClaimsAuthenticationManagerStub : ClaimsAuthenticationManager
    //{
    //    public override ClaimsPrincipal Authenticate(string resourceName, ClaimsPrincipal incomingPrincipal)
    //    {
    //        var newPrincipal = new ClaimsPrincipal(incomingPrincipal);

    //        var id = new ClaimsIdentity("ClaimsAuthenticationManager");
    //        id.AddClaim(new Claim(ClaimTypes.Role, "RoleFromClaimsAuthManager", null, "ClaimsAuthenticationManagerStub"));
    //        newPrincipal.AddIdentity(id);

    //        return newPrincipal;
    //    }
    //}
}