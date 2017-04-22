namespace BaristaLabs.Skrapr.Definitions
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public abstract class AuthenticatorBase : IAuthenticator
    {
        /// <summary>
        /// Gets or sets the name of the authentcation.
        /// </summary>
        [JsonProperty("type", Required = Required.Always)]
        public abstract string Type
        {
            get;
        }

        /// <summary>
        /// The username to supply to the authentication request
        /// </summary>
        [JsonProperty("username", Required = Required.Always)]
        public string Username
        {
            get;
            set;
        }

        /// <summary>
        /// The password to supply to the authentication request
        /// </summary>
        [JsonProperty("password", Required = Required.Always)]
        public string Password
        {
            get;
            set;
        }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData
        {
            get;
            set;
        }
    }

    public class BasicAuthenticator : AuthenticatorBase
    {
        public override string Type
        {
            get { return "basic"; }
        }

        [JsonProperty("realm", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(null)]
        public string Realm
        {
            get;
            set;
        }
    }

    public class DigestAuthenticator : AuthenticatorBase
    {
        public override string Type
        {
            get { return "digest"; }
        }

        [JsonProperty("realm", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(null)]
        public string Realm
        {
            get;
            set;
        }
    }

    public class NtlmAuthenticator : AuthenticatorBase
    {
        public override string Type
        {
            get { return "ntlm"; }
        }

        [JsonProperty("workstation", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(null)]
        public string Workstation
        {
            get;
            set;
        }

        [JsonProperty("domain", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(null)]
        public string Domain
        {
            get;
            set;
        }
    }

    public class FormAuthenticator : AuthenticatorBase
    {
        public override string Type
        {
            get { return "form"; }
        }

        /// <summary>
        /// Gets or sets a value that indicates if jQuery should be injected on the page, if it doesn't already exist. (Optional - Default is that jQuery is not required)
        /// </summary>
        [JsonProperty("requireJQuery", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        public bool RequireJQuery
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the script that will be executed in the context of the page to determine if the current request is authenticated.
        /// </summary>
        /// <remarks>
        /// Must always be a function with the signature function(username) {...} that returns a boolean value.
        /// </remarks>
        [JsonProperty("isAuthenticatedScript", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string IsAuthenticatedScript
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the script that will be executed in the context of the page to perform the actual authentication
        /// </summary>
        /// <remarks>
        /// Must always be a function with the signature function(username, password) {...}.
        /// </remarks>
        [JsonProperty("authenticationScript", Required = Required.Always)]
        public string AuthenticationScript
        {
            get;
            set;
        }
    }
}
