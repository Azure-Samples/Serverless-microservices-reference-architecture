using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ServerlessMicroservices.Shared.Services
{
    public interface IUserService
    {
        Task<(User, string error)> CreateUser(CreateUser newUser);
        Task<(IEnumerable<User>, string error)> GetUsers();
        Task<(User, string error)> GetUserById(string userId);
    }

    public class UsersResult
    {
        public IEnumerable<User> Value { get; set; }
    }

    public class User
    {
        public string objectId { get; set; }

        public string displayName { get; set; }
        public string givenName { get; set; }
        public string surname { get; set; }

        public string streetAddress { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postalCode { get; set; }
        public string country { get; set; }

        public IEnumerable<SignInName> signInNames { get; set; }
        public IEnumerable<string> otherMails { get; set; }

        public string Email =>
            signInNames?.FirstOrDefault(x => x.type == "emailAddress")?.value ??
            otherMails?.FirstOrDefault();
    }

    public class CreateUser
    {
        public CreateUser(string email, string password, string displayName)
        {
            signInNames = new SignInName[]
            {
                new SignInName{type = "emailAddress", value = email}
            };
            passwordProfile = new PasswordProfile
            {
                password = password
            };
            this.displayName = displayName;
        }

        public string displayName { get; set; }
        public string givenName { get; set; }
        public string surname { get; set; }

        public string streetAddress { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postalCode { get; set; }
        public string country { get; set; }

        public IEnumerable<SignInName> signInNames { get; private set; }
        public bool accountEnabled { get; private set; } = true;
        public string creationType { get; private set; } = "LocalAccount";
        public PasswordProfile passwordProfile { get; private set; }
        public string passwordPolicies { get; private set; } = "DisablePasswordExpiration";
    }

    public class PasswordProfile
    {
        public string password { get; set; }
        public bool forceChangePasswordNextLogin { get; set; } = false;
    }

    public class SignInName
    {
        public string type { get; set; }
        public string value { get; set; }
    }

    public class BadRequestResponse
    {
        [JsonProperty(PropertyName = "odata.error")]
        public Error Error { get; set; }

        public string ErrorMessage => Error.Message.Value;
    }

    public class Error
    {
        public string code { get; set; }
        public Message Message { get; set; }
    }

    public class Message
    {
        public string Value { get; set; }
    }
}
