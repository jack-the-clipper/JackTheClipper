using System;
using System.Runtime.Serialization;

namespace JackTheClipperCommon.SharedClasses
{
    /// <summary>
    /// Basic user information
    /// </summary>
    [DataContract]
    public class BasicUserInformation
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        [DataMember(Name = "UserId")]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        [DataMember(Name = "UserName")]
        public string UserName { get; private set; }

        /// <summary>
        ///Gets a value indicating whether the user is valid or not.
        /// </summary>
        [DataMember(Name = "UserValid")]
        public bool IsValid { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicUserInformation"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="isValid">if set to <c>true</c> the user is valid.</param>
        public BasicUserInformation(Guid id, string userName, bool isValid)
        {
            Id = id;
            UserName = userName;
            IsValid = isValid;
        }
    }
}