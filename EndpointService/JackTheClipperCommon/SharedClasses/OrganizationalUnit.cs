using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace JackTheClipperCommon.SharedClasses
{
    /// <summary>
    /// Contains the definition of an Organization
    /// Contains an Id, a name, Parent Organization
    /// </summary>
    [DataContract]
    public class OrganizationalUnit : IEquatable<OrganizationalUnit>
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
        /// Gets the parent unit.
        /// </summary>
        [DataMember(Name = "ParentId")]
        public Guid ParentId { get; private set; }

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        [DataMember(Name = "Children")]
        public IReadOnlyCollection<OrganizationalUnit> Children { get; set; }

        /// <summary>
        /// Gets a value indicating whether this unit is a principal (="mandant") unit.
        /// </summary>
        [DataMember(Name = "IsPrincipalUnit")]
        public bool IsPrincipalUnit { get; private set; }

        /// <summary>
        /// Gets the identifier of the belonging <see cref="OrganizationalUnitSettings"/>.
        /// </summary>
        [DataMember(Name = "SettingsId")]
        public Guid SettingsId { get; private set; }

        /// <summary>
        /// Flattens the tree hierarchy of an <see cref="OrganizationalUnit"/> and its <see cref="Children"/>
        /// </summary>
        /// <returns>List of all <see cref="OrganizationalUnit"/>s below this unit</returns>
        public IEnumerable<OrganizationalUnit> GetAllChildren()
        {
            return Children.Concat(Children.SelectMany(unit => unit.GetAllChildren()));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationalUnit"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="isPrincipalUnit">Flag, indicating whether this unit is a client or not.</param>
        /// <param name="parentId">The id of the parent organizational unit.</param>
        /// <param name="adminMailAddress">The email address of the staffchief for this unit</param>
        /// <param name="settingsId">The id of default settings.</param>
        public OrganizationalUnit(Guid id, [NotNull] string name, bool isPrincipalUnit,
                                  string adminMailAddress, Guid parentId, Guid settingsId)
        {
            Id = id;
            Name = name;
            IsPrincipalUnit = isPrincipalUnit;
            AdminMailAddress = adminMailAddress;
            ParentId = parentId;
            SettingsId = settingsId;
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
        public bool Equals(OrganizationalUnit other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OrganizationalUnit)obj);
        }

        /// <summary>Serves as the default hash function.</summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}