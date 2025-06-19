using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinRep_Code.Src
{
    public class Packeg
    {
        private string Name { get; }
        private string Version { get; }
        private string Description { get; }
        private string Id { get; }

        private string Setter(string value, string propertyName, int maxLength = 32)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"{propertyName} cannot be null or empty.", nameof(value));
            }
            if (value.Length > maxLength)
            {
                throw new ArgumentException($"{propertyName} cannot exceed {maxLength} characters.", nameof(value));
            }
            return value;
        }

        public Packeg(string name, string version, string description, string id)
        {
            Name = Setter(name, nameof(name));
            Version = Setter(version, nameof(version));
            Description = Setter(description, nameof(description), 512);
            Id = Setter(id, nameof(id));
        }
    }
}
