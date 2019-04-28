using System.Reflection;
using JackTheClipperCommon.Enums;
using JackTheClipperCommon.Interfaces;
using JackTheClipperCommon.SharedClasses;
using JackTheClipperData;

namespace JackTheClipperBusiness
{
    public class StatusController : IClipperService
    {
        public MethodResult GetStatus()
        {
            return new MethodResult(SuccessState.Successful, "Version: " + Assembly.GetCallingAssembly().GetName().Version);
        }
    }
}