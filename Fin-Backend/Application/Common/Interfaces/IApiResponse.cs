using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinTech.Application.Common.Interfaces
{
    public interface IApiResponse
    {
        bool Succeeded { get; set; }
        string Message { get; set; }
        List<string> Errors { get; set; }
    }
}
