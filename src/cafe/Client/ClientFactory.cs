﻿using cafe.Options;
using NLog;
using RestEase;

namespace cafe.Client
{
    public class ClientFactory : IClientFactory
    {
        private static readonly Logger Logger = LogManager.GetLogger(typeof(ClientFactory).FullName);

        private readonly string _hostname;
        private readonly int _port;

        public ClientFactory(string hostname, int port)
        {
            Logger.Debug($"Creating clients on port {port}");
            _hostname = hostname;
            _port = port;
        }

        public IChefServer RestClientForChefServer()
        {
            return CreateRestClientFor<IChefServer>("chef");
        }

        public IJobServer RestClientForJobServer()
        {
            return CreateRestClientFor<IJobServer>("job");
        }


        private T CreateRestClientFor<T>(string serviceEndpoint)
        {
            var endpoint = $"http://{_hostname}:{_port}/api/{serviceEndpoint}";
            Logger.Debug($"Creating rest client for {typeof(T).FullName} at endpoint {endpoint}");
            return RestClient.For<T>(endpoint);
        }
    }
}