using Grpc.Net.Client.Balancer;

namespace ApiGateway
{
    public class DiskResolver : Resolver
    {
        private readonly Uri _address;
        private Action<ResolverResult> _listener;

        public DiskResolver(Uri address)
        {
            _address = address;
        }

        public override async void Refresh()
        {
            var addresses = new List<BalancerAddress>();

            foreach (var line in File.ReadLines(_address.Host))
            {
                var addresComponents = line.Split(' ');
                addresses.Add(new BalancerAddress(addresComponents[0], int.Parse(addresComponents[1])));
            }

            _listener(ResolverResult.ForResult(addresses));
        }

        public override void Start(Action<ResolverResult> listener)
        {
            _listener = listener;
        }
    }

    public class DiskResolverFactory : ResolverFactory
    {
        public override string Name => "disk";

        public override Resolver Create(ResolverOptions options)
        {
            return new DiskResolver(options.Address);
        }
    }
}