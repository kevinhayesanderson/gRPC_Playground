using Google.Protobuf.WellKnownTypes;
using Users;

namespace UserInfoManager
{
    public class UserDataCache
    {
        private readonly List<UserInfo> users;

        public UserDataCache()
        {
            users =
            [
                new UserInfo
                {
                    FirstName = "John",
                    Surname = "Smith",
                    Gender = "M",
                    DateOfBirth = Timestamp.FromDateTime(DateTime.UtcNow.AddYears(-20)),
                    Nationality = "English",
                    Address = new AddressInfo
                    {
                        FirstLine = "51 Park Lane",
                        PostcodeOrZipCode = "SW2 5BL",
                        Town = "London",
                        Country = "UK"
                    }
                },
            ];
        }

        public IEnumerable<UserInfo> GetUsers()
        {
            return users;
        }
    }
}