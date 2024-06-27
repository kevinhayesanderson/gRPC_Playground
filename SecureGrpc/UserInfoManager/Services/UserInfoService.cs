using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Users;

namespace UserInfoManager.Services
{
    public class UserInfoService(UserDataCache userDataCache) : UserManager.UserManagerBase
    {
        public override async Task GetAllUsers(Empty request,
                                               IServerStreamWriter<UserInfo> responseStream,
                                               ServerCallContext context)
        {
            Console.WriteLine($"Client authenticated: {context.AuthContext.IsPeerAuthenticated}");
            if (context.AuthContext.IsPeerAuthenticated)
            {
                Console.WriteLine($"Auth property name:{context.AuthContext.PeerIdentityPropertyName}");
                Console.WriteLine($"Auth property value:{context.AuthContext.Properties.FirstOrDefault()?.Value}");
            }

            foreach (var item in userDataCache.GetUsers())
            {
                await responseStream.WriteAsync(item);
            }
        }
    }
}