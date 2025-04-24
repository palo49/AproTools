using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AproTools
{
    public class DesignHelper
    {
        public static bool IsInRuntime => !Design.IsDesignMode;
        public static bool IsInDesign => Design.IsDesignMode;
    }
}
