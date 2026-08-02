using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Legend2Toolbox.Domain.Models;

public class RemoveContentListCommand
{
    public string FilePath { get; set; } = string.Empty;
    public List<string> ContentList { get; set; } = [];
    public string LogMessage { get; set; } = string.Empty;
}
