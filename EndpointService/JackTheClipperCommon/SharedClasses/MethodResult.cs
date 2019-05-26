using System.Runtime.Serialization;
using JackTheClipperCommon.Enums;

namespace JackTheClipperCommon.SharedClasses
{
    /// <summary>
    /// Used to return if a Method was Successful or not. 
    /// </summary>
    [DataContract]
    public class MethodResult
    {
        /// <summary>
        /// Gets the status.
        /// </summary>
        [IgnoreDataMember]
        public SuccessState Status { get; private set; }

        /// <summary>
        /// Gets the status as string.
        /// </summary>
        [DataMember(Name = "Status")]
        public string StatusAsString => Status.ToString();

        /// <summary>
        /// Gets the user message (if any).
        /// </summary>
        [DataMember(Name = "UserMessage")]
        public string UserMessage { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodResult"/> class.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="userMessage">The user message.</param>
        public MethodResult(SuccessState status, string userMessage)
        {
            Status = status;
            UserMessage = userMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodResult"/> class.
        /// </summary>
        public MethodResult()
        {
        }
    }

    /// <summary>
    /// Used to return whether a method was successful or not. If successful the result of the method is included
    /// </summary>
    /// <typeparam name="T">The result of the method</typeparam>
    public class MethodResult<T> : MethodResult
    {
        /// <summary>
        /// The result of the method
        /// </summary>
        public T Result { get; private set; }

        /// <summary>
        /// Instantiates a new instance of the <see cref="MethodResult{T}"/> class
        /// </summary>
        /// <param name="result">The result of the method</param>
        public MethodResult(T result)
        {
            Result = result;
        }

        /// <summary>
        /// Instantiates a new instance of the <see cref="MethodResult{T}"/> class
        /// </summary>
        /// <param name="status">The status</param>
        /// <param name="userMessage">The user message</param>
        /// <param name="result">The result of the method</param>
        public MethodResult(SuccessState status, string userMessage, T result) : base(status, userMessage)
        {
            Result = result;
        }
    }
}