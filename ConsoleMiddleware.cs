using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace GitDocs
{
    public class ConsoleMiddleware : OwinMiddleware
    {
        public ConsoleMiddleware(OwinMiddleware next)
            : base(next)
        {

        }

        public override async Task Invoke(IOwinContext context)
        {
            try
            {
                await Next.Invoke(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                if (Debugger.IsAttached)
                    Debugger.Break();
            }
        }
    }
}
