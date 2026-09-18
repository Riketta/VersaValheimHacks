namespace VersaValheimHacks.Options
{
    internal class ContainerOptions
    {
        /// <summary>
        /// Add a "Sort" button below the container panel's "Place stacks"
        /// button; clicking it sorts the open chest alphabetically. Applied
        /// when the inventory GUI is built, so a change needs a game restart.
        /// </summary>
        public bool SortButton { get; set; } = true;
    }
}
