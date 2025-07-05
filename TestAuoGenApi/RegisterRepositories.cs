using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAutoGen.Dal
{
    public partial class RegisterRepositories
    {
        public void Register(IServiceCollection services)
        {
            RegisterGeneratedRepositories(services); // Always compiles
        }

        static partial void RegisterGeneratedRepositories(IServiceCollection services);
    }

}
