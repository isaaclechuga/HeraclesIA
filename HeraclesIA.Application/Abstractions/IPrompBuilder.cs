using HeraclesIA.Types.Prompts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeraclesIA.Application.Abstractions
{
    public interface IPromptBuilder
    {
        string Build(PromptType type, string data, string data2 = "");
    }
}
