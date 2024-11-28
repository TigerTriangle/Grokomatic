namespace Grokomatic.Models
{
    public class TechInnovation
    {
        /// <summary>
        /// The category of the technology (e.g., Hardware, Software, Robotics).
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// The name or common designation of the innovation.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A brief description of the technology and its significance or functionality.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Constructor for TechInnovation class.
        /// </summary>
        /// <param name="category">The category of the tech innovation.</param>
        /// <param name="name">The name of the tech innovation.</param>
        /// <param name="description">A description of the tech innovation.</param>
        public TechInnovation(string category, string name, string description)
        {
            Category = category;
            Name = name;
            Description = description;
        }

        /// <summary>
        /// Returns a string representation of the TechInnovation object.
        /// </summary>
        /// <returns>A formatted string containing the category, name, and description of the innovation.</returns>
        public override string ToString()
        {
            return $"{Name} - Category: {Category}\nDescription: {Description}";
        }

        // Override Equals and GetHashCode for comparison
        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            TechInnovation other = (TechInnovation)obj;
            return Name == other.Name && Category == other.Category && Description == other.Description;
        }

        public override int GetHashCode()
        {
            return (Name, Category, Description).GetHashCode();
        }
    }
}
