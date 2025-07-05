using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinRep_Code.Src
{
    interface ITweakProvider
    {
        public IEnumerable<string> GetTweaks();
        public bool TryApplyTweakByName(string name);
        public bool TryRevertTweakByName(string name);  
    }
}
