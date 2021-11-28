namespace TwoFactorAuth
{
    public class Token
    {
        public string TheToken { get; set; }
        public Token(string theToken)
        {
            TheToken = theToken;
        }
    }
}