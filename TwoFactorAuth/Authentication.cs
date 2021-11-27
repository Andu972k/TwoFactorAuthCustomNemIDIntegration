namespace TwoFactorAuth
{
    public class Authentication
    {
        public string JWToken { get; set; }
        public string Code { get; set; }

        public Authentication(string jwtoken, string code)
        {
            JWToken = jwtoken;
            Code = code;
        }
    }
}