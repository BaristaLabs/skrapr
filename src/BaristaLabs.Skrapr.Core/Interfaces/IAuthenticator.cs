namespace BaristaLabs.Skrapr
{
    using Newtonsoft.Json;

    public interface IAuthenticator
    {
        /// <summary>
        /// Gets or sets the name of the authentcation.
        /// </summary>
        [JsonProperty("type")]
        string Type
        {
            get;
        }

        /// <summary>
        /// The username to supply to the authentication request
        /// </summary>
        [JsonProperty("username")]
        string Username
        {
            get;
            set;
        }

        /// <summary>
        /// The password to supply to the authentication request
        /// </summary>
        [JsonProperty("password")]
        string Password
        {
            get;
            set;
        }
    }
}
