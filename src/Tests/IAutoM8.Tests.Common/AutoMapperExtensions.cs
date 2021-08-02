using AutoMapper;
using System;
using System.Linq;

namespace IAutoM8.Tests.Common
{
    public sealed class TestAutoMapper
    {
        private static readonly Lazy<IMapper> lazy =
            new Lazy<IMapper>(() =>
            {
                var automapperProfiles = typeof(Service.Infrastructure.Module).Assembly.GetTypes()
                    .Where(type => type.IsSubclassOf(typeof(Profile)));

                var cfg = new MapperConfiguration(config =>
                {
                    foreach (var automapperProfile in automapperProfiles)
                    {
                        config.AddProfile(automapperProfile);
                    }
                });

                return cfg.CreateMapper();
            });

        public static IMapper Instance => lazy.Value;
    }
}
