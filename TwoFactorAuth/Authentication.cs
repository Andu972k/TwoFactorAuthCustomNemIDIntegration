namespace TwoFactorAuth
{
    public class Authentication
    {
        public string JWToken { get; set; }
        public string Code { get; set; }

        /// <summary>
        /// class for storing info from a user trying to authenticate.
        /// </summary>
        /// <param name="jwtoken">the Json Web Token</param>
        /// <param name="code">their 4 digit code</param>
        public Authentication(string jwtoken, string code)
        {
            JWToken = jwtoken;
            Code = code;
        }
    }
}