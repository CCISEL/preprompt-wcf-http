using System;
using System.Reflection;

namespace PrePrompt.Samples.Common
{
    public class WebLinkTarget : IEquatable<WebLinkTarget>
    {
        public WebLinkTarget(MethodInfo method, string uri, string relationType)
        {
            Method = method;
            Uri = uri;
            RelationType = relationType;
        }

        public MethodInfo Method { get; set; }
        public string Uri { get; set; }
        public string RelationType { get; set; }

        public WebLinkTarget Clone()
        {
            return new WebLinkTarget(Method, Uri, RelationType);
        }

        public bool Equals(WebLinkTarget other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(other.Method, Method) 
                && other.Uri.CompareToIgnoreCase(Uri) == 0
                && other.RelationType.CompareToIgnoreCase(RelationType) == 0;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == typeof (WebLinkTarget) && Equals((WebLinkTarget) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = Method.GetHashCode();
                result = (result*397) ^ Uri.GetHashCode();
                result = (result*397) ^ RelationType.GetHashCode();
                return result;
            }
        }

        public static bool operator ==(WebLinkTarget left, WebLinkTarget right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(WebLinkTarget left, WebLinkTarget right)
        {
            return !Equals(left, right);
        }
    }
}