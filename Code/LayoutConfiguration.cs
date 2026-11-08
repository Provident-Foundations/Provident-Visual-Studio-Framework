using System;
using System.Collections.Generic;
using System.Text;

namespace Provident.Foundations.VisualStudio.Code
{
    /// <summary>
    /// Describes the ordered regions used when laying out a class declaration.
    /// </summary>
    public sealed class LayoutConfiguration
    {
        /// <summary>
        /// Gets the configured regions in output order.
        /// </summary>
        /// <value>A mutable list of region configurations.</value>
        public IList<RegionConfiguration> Regions { get; init; }
        = new List<RegionConfiguration>();
    }

    }
