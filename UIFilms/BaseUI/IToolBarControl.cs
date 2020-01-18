using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BaseUI
{
    interface IToolBarControl
    {
        void AddToToolBar(ToolStripItem item);
    }
}
