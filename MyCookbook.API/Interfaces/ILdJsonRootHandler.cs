using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MyCookbook.API.Interfaces;

public interface ILdJsonRootHandler
{
    public ValueTask Process(
        Uri url,
        IReadOnlyList<string> data,
        CancellationToken cancellationToken);
}