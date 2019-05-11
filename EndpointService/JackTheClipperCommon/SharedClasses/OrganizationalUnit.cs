using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace JackTheClipperCommon.SharedClasses
{
    /// <summary>
    /// Contains the definition of an Organization
    /// Contains an Id, a name, Parent Organization
    /// </summary>
    [DataContract]
    public class OrganizationalUnit
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        [DataMember(Name = "OrganizationalUnitId")]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        [NotNull]
        [DataMember(Name = "OrganizationalUnitName")]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the belonging admin's mail address.
        /// </summary>
        [DataMember(Name = "AdminMailAddress")]
        public string AdminMailAddress { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this unit is a customer root (="mandant") unit.
        /// </summary>
        [IgnoreDataMember]
        public bool CustomerRoot { get; private set; }

        /// <summary>
        /// Gets the default settings of the unit.
        /// </summary>
        [IgnoreDataMember]
        public OrganizationalUnitSettings DefaultSettings { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationalUnit"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="customerRoot">Flag, indicating whether mandant.</param>
        /// <param name="parentOrganizationalUnit">The parent organizational unit.</param>
        /// <param name="defaultSettings">The default settings.</param>
        public OrganizationalUnit(Guid id, [NotNull] string name, bool customerRoot,
                                  OrganizationalUnitSettings defaultSettings, string adminMailAddress)
        {
            Id = id;
            Name = name;
            CustomerRoot = customerRoot;
            DefaultSettings = defaultSettings;
            AdminMailAddress = adminMailAddress;
        }
    }
}