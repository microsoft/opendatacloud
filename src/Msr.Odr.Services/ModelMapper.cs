using System;
using Boxed.Mapping;

namespace Msr.Odr.Services
{
    public static class ModelMapper<T>
        where T : new()
    {
        public static T Mapper { get; } = new T();
    }
}
