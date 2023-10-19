using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllDone.Infra.Dispatch
{
    internal class LoggingMiddleware : IDispatchMiddleware
    {
        private readonly IDispatchMiddleware _next;

        public LoggingMiddleware(IDispatchMiddleware next)
        {
            _next = next;
        }

        public async Task<object> ExecuteOperationAsync(object request)
        {
            // log request

            var response = await _next.ExecuteOperationAsync(request);

            // log response

            return response;
        }
    }
}
