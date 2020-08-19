using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Dapper.Base
{

    public interface IDapperFactoryBuilder
    {
        string Name { get; }

        IServiceCollection Services { get; }
    }
}
