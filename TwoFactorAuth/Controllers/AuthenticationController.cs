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
            //generate code - 4 digit ---- it is string ---
            var rand = new Random();
            string code = "";
            for(int i = 0; i<=3; i++)
            {
                code = code +rand.Next(10);
            }
            //add user and code to code dictionary
            CodeDic.Add(body, code);
            //add user with tries to attempt dictionary
            AttemptDic.Add(body, 0);
            //send generated code to phone via FatSMS

            //packet to send to fatsms
            var packet = new Dictionary<string, string>
            {
                { "to_phone", phoneNumber },
                { "message", code },
                {"api_key", apikey }
            };
            
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
                if (entry.Key != auth.JWToken)
                {
                    continue;
                }
                if (entry.Value == auth.Code)
                {
                    //return answer depending on whether they are the same or not
                    ret = true;
                }
                else
                {
                    //erase current code if: 3 wrong answers or correct answer
                }
            }
            
            return ret;
        }
        private void CodeCount(string Key)
        {
            AttemptDic[Key] = AttemptDic[Key] + 1;
            if (AttemptDic[key] >= 3)
            {
                //AttemptDic.Remove
            }
        }
    }
}
