using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vxk165930Asg4
{
    /*
     * This class stores the entire line and line number where the string appears
     * for every search
     */
    class Results
    {
        private int lineNo;
        private String lineText;

        public int LineNo { get => lineNo; set => lineNo = value; }
        public string LineText { get => lineText; set => lineText = value; }
    }
}
