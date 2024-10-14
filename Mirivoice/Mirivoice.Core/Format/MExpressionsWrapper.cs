using Avalonia.Threading;
using Mirivoice.Engines;
using Mirivoice.ViewModels;
using Mirivoice.Views;
using R3;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VYaml.Annotations;
using VYaml.Serialization;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.Mirivoice.Plugins.Builtin.Phonemizers;

namespace Mirivoice.Mirivoice.Core.Format
{
    [YamlObject]
    public partial class MExpressionsWrapper
    {
        // For VITS2
        public float VITS2Speed { get; set; } = 1.0f;
        public float VITS2Noise1 { get; set; } = 0.667f;
        public float VITS2Noise2 { get; set; } = 0.8f;

        public MExpressionsWrapper()
        {
               
        }
    }
}
