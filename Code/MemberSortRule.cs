namespace Provident.Foundations.VisualStudio.Code
{
    /// <summary>
    /// Defines a rule that can be used to order members within a layout region.
    /// </summary>
    public enum MemberSortRule
    {
        /// <summary>Sorts names in ascending ordinal order.</summary>
        NameAscending,
        /// <summary>Sorts names in descending ordinal order.</summary>
        NameDescending,

        /// <summary>Places static members first.</summary>
        StaticFirst,
        /// <summary>Places static members last.</summary>
        StaticLast,

        /// <summary>Places readonly members first.</summary>
        ReadonlyFirst,
        /// <summary>Places readonly members last.</summary>
        ReadonlyLast,

        /// <summary>Places asynchronous methods first.</summary>
        AsyncFirst,
        /// <summary>Places asynchronous methods last.</summary>
        AsyncLast,

        /// <summary>Places virtual members first.</summary>
        VirtualFirst,
        /// <summary>Places virtual members last.</summary>
        VirtualLast,

        /// <summary>Places overridden members first.</summary>
        OverrideFirst,
        /// <summary>Places overridden members last.</summary>
        OverrideLast,

        /// <summary>Places constructors first.</summary>
        ConstructorsFirst,
        /// <summary>Places constructors last.</summary>
        ConstructorsLast,

        /// <summary>Sorts by ascending parameter count.</summary>
        ByParameterCount,
        /// <summary>Sorts by ascending source line count.</summary>
        ByLineCount
    }

}
