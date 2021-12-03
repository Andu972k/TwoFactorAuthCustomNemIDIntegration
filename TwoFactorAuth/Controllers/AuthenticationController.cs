using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

namespace TwoFactorAuth.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        //Memory space for authentication codes. key = JWT. value = Code.
        private static Dictionary<string, string> CodeDic = new Dictionary<string, string>();
        //Attempt dictionary
        private static Dictionary<string, int> AttemptDic = new Dictionary<string, int>();
        //API key for fatSMS
        private string apikey = "fe8f0664-f2ac-40fa-bf91-b4506ecf3d90";
        //phonenumber to send message for fatSMS
        private string phoneNumberJoachim = "53447600";
        private string phoneNumberAnders = "60774554";
        //HTTP client for requests
        private static readonly HttpClient client = new HttpClient();

        [HttpPost]
        [Route("Code")]
        public async Task GenerateCode([FromBody]string JWT)
        {
            //generate 4 digit code ---- it is a string of numbers---
            var rand = new Random();
            string code = "";
            for(int i = 0; i<=3; i++)
            {
                code = code +rand.Next(10);
            }
            //add user(JWT) and their code(4 digit code) to code dictionary
            if (JWT == "" || JWT == null)
            {
                throw new Exception("invalid token");
            }
            try{
                CodeDic.Add(JWT, code);
            }
            catch (ArgumentException)
            {
                throw new Exception("User already exist");
            }
            
            //add user with tries to attempt dictionary
            AttemptDic.Add(JWT, 0);
 
            /*
            Maybe add a function to delete the earlist user when amount surpasses a threshold?
            */

           //make form content of JSON type to send to FatSMS. includes new code.
            var formcontent = new FormUrlEncodedContent(new []
            {
                new KeyValuePair<string, string>("to_phone", phoneNumberAnders),
                new KeyValuePair<string, string>("message", code),
                new KeyValuePair<string, string>("api_key", apikey)
            });

            //Make http request to fatSMS
            try
            {
                HttpResponseMessage response = await client.PostAsync("https://fatsms.com/send-sms", formcontent);
            }
            catch (Exception e)
            {
                Console.WriteLine("error type " + e.ToString() + " has occurred");
                
                //send email? SMTP?
            }
            
        
            //success?
        }

        [HttpPost]
        [Route("Auth")]
        public int compareCode([FromBody]Authentication auth){
            //compare code in memory with given code
            int ret = 1;
            foreach (var entry in CodeDic)
            {
                //Find user in memory
                if (entry.Key != auth.JWToken)
                {
                    continue;
                }
                //when user found: check if correct code
                if (entry.Value == auth.Code)
                {
                    //ret becomes 1 if answer is correct
                    ret = 0;
                    //erase user from memory
                    Codecheck(entry.Key, ret);
                    break;
                }
                else
                {
                    //erase user and code if 3 wrong answers. 3 strikes you are out!
                   ret = Codecheck(entry.Key, ret);
                   break;
                }
            }
            //returns 0 if registered user with correct code. 1 if wrong- user or code. 2 if no more tries left.
            return ret;
            
        }

        /// <summary>
        /// method for when a registered user has tried to login
        /// will delete user if 3 wrong attempts.
        /// Also deletes user info if correct code.
        /// </summary>
        /// <param name="key">The users JWT(JSON Web Token)</param>
        /// <param name="attempt">Did the user use corect code?</param>
        private int Codecheck(string key, int attempt)
        {
            //add one to their attempts
            AttemptDic[key] ++;
            //check if they should be deleted
            if (AttemptDic[key] >= 3 )
            {
                AttemptDic.Remove(key);
                CodeDic.Remove(key);
                return 2;
            }
            else if(attempt == 0){
                AttemptDic.Remove(key);
                CodeDic.Remove(key);
                return 0;
            }
            return 1;
        }
    }
}
