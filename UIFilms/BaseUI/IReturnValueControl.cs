using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseUI
{
    public interface IReturnValueControl
    {
        event EventHandler Selected;

        object Value { get; }
    }
}
