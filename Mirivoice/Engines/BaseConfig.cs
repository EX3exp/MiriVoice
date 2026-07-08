using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirivoice.Engines
{
    // Base configuration class for all engine configurations
    public abstract class BaseConfig
    {
        public virtual string ModelPath { get; set; }
    }
}
