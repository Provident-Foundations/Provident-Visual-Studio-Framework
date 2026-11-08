using System;
using System.Collections.Generic;
using System.Text;

namespace Provident.Foundations.VisualStudio.Code
{
    /// <summary>
    /// Describes one output region and the rules used to order its members.
    /// </summary>
    public sealed class RegionConfiguration
    {
        /// <summary>
        /// Gets the display name written in the region directive.
        /// </summary>
        /// <value>The region name.</value>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Gets the member category assigned to this region.
        /// </summary>
        /// <value>The category handled by the region.</value>
        public MemberCategory Category { get; init; }

        /// <summary>
        /// Gets the stable, primary-to-secondary sort rules for the region.
        /// </summary>
        /// <value>The configured sort rules.</value>
        public IList<MemberSortRule> SortRules { get; init; }= new List<MemberSortRule>();
    }
}
