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
        Dictionary<string, string> CodeDic = new Dictionary<string, string>();
        //Attempt dictionary
        Dictionary<string, int> AttemptDic = new Dictionary<string, int>();
        //API key for fatSMS
        string apikey = "fe8f0664-f2ac-40fa-bf91-b4506ecf3d90";
        //phonenumber to send message for fatSMS
        string phoneNumber = "53447600";
        //HTTP client for requests
        private static readonly HttpClient client = new HttpClient();

        [HttpPost]
        [Route("Code")]
        public async Task GenerateCode([FromBody]string body)
        {
            //generate 4 digit code ---- it is a string of numbers---
            var rand = new Random();
            string code = "";
            for(int i = 0; i<=3; i++)
            {
                code = code +rand.Next(10);
            }
            //add user(JWT) and their code(4 digit code) to code dictionary
            CodeDic.Add(body, code);
            //add user with tries to attempt dictionary
            AttemptDic.Add(body, 0);

            /*
            Maybe add a function to delete the earlist user when amount surpasses a threshold?
            */



            //send generated code to phone via FatSMS

            //packet to send to fatsms
            var packet = new Dictionary<string, string>
            {
                { "to_phone", phoneNumber },
                { "message", code },
                {"api_key", apikey }
            };
            //serialize content packet to send via http request
            HttpContent content = new StringContent(JsonSerializer.Serialize(packet), Encoding.UTF8, "application/json");
            //Make http request to fatSMS
            try
            {
                HttpResponseMessage response = await client.PostAsync("https://fatsms.com/send-sms", content);
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
        public bool compareCode([FromBody]Authentication auth){
            //compare code in memory with given code
            bool ret = false;
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
                    //ret becomes true if answer is correct
                    ret = true;
                    //erase user from memory
                    Codecheck(entry.Key, ret);
                }
                else
                {
                    //erase user and code if 3 wrong answers. 3 strikes you are out!
                    Codecheck(entry.Key, ret);
                }
            }
            //returns true if user was found AND code was correct, otherwise false.
            return ret;
        }

        /// <summary>
        /// method for when a registered user has tried to login
        /// will delete user if 3 wrong attempts.
        /// Also deletes user info if correct code.
        /// </summary>
        /// <param name="key">The users JWT(JSON Web Token)</param>
        /// <param name="attempt">Did the user use corect code?</param>
        private void Codecheck(string key, bool attempt)
        {
            //add one to their attempts
            AttemptDic[key] = AttemptDic[key] + 1;
            //check if they should be deleted
            if (AttemptDic[key] >= 3 || attempt == true)
            {
                AttemptDic.Remove(key);
                CodeDic.Remove(key);
            }
        }
    }
}
