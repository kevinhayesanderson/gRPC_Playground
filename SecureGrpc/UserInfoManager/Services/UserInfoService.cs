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
            foreach (var item in userDataCache.GetUsers())
            {
                await responseStream.WriteAsync(item);
            }
        }
    }
}