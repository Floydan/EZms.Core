using System;
using System.ComponentModel;
using System.Globalization;

namespace EZms.Core.Models
{
    [TypeConverter(typeof(ContentReferenceConverter))]
    [Serializable]
    public class ContentReference : IComparable, IComparable<ContentReference>, IEquatable<ContentReference>
    {
        private int _contentId;
        private int _versionId;
        private bool _ignoreWorkId;

        public int Id
        {
            get => _contentId;
            set => _contentId = value;
        }

        public int WorkId
        {
            get => _versionId;
            set => _versionId = value;
        }

        /// <summary>
        /// Returns an empty <see cref="T:EZms.Core.Models.ContentReference" />.
        /// </summary>
        public static readonly ContentReference EmptyReference = new ContentReference();

        /// <summary>
        /// Returns a <see cref="T:EZms.Core.ContentReference" /> that references the current Content.
        /// </summary>
        public static readonly ContentReference SelfReference = new ContentReference(0, -1);

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EZms.Core.Models.ContentReference" /> class.
        /// </summary>
        public ContentReference()
        {
        }

        /// <summary>
        /// Initialize a new <see cref="T:EZms.Core.Models.ContentReference" /> with content id.
        /// </summary>
        /// <param name="contentId">The content ID.</param>
        public ContentReference(int contentId)
        {
            _contentId = contentId;
        }

        /// <summary>
        /// Initialize a new <see cref="T:EZms.Core.Models.ContentReference" /> from a string in the format
        ///     contentID[_workID] or -
        /// throws Exception on invalid argument
        /// </summary>
        /// <param name="complexReference">The string containing content information</param>
        /// <exception cref="T:System.Exception">
        /// Thrown if the string cannot be parsed as a valid ContentReference.
        /// </exception>
        public ContentReference(string complexReference)
        {
            if (string.IsNullOrEmpty(complexReference))
                throw new Exception("ContentReference string cannot be null/empty");
            var contentReference = Parse(complexReference);
            _contentId = contentReference._contentId;
            _versionId = contentReference._versionId;
        }

        /// <summary>
        /// Initialize a new <see cref="T:EZms.Core.Models.ContentReference" /> with content id and working version.
        /// </summary>
        /// <param name="contentId">The content ID.</param>
        /// <param name="versionId">The version ID.</param>
        public ContentReference(int contentId, int versionId) : this(contentId)
        {
            _versionId = versionId;
        }

        public int CompareTo(ContentReference other)
        {
            return _contentId.CompareTo(other._contentId);
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return -1;

            if (!(obj is ContentReference other))
                throw new ArgumentException($"Object is not a {nameof(ContentReference)}");

            return CompareTo(other);
        }

        public bool Equals(ContentReference other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return _contentId == other._contentId;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ContentReference)obj);
        }

        public override int GetHashCode()
        {
            var hashCodeCombiner = new Microsoft.DotNet.PlatformAbstractions.HashCodeCombiner();
            hashCodeCombiner.Add(Id);
            if (!_ignoreWorkId)
                hashCodeCombiner.Add(WorkId);
            return hashCodeCombiner.CombinedHash;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// The return value can be be a string like "Digit[_Digit]" or "-"
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// The ContentReference format can be a string like "Digit[_Digit]", "-" or String.Empty
        /// </returns>
        public override string ToString()
        {
            if (_contentId == 0)
            {
                return _versionId != -1 ? string.Empty : "-";
            }
            var str = _contentId.ToString(CultureInfo.InvariantCulture);
            if (_versionId != 0)
                str = str + "_" + _versionId.ToString(CultureInfo.InvariantCulture);
            return str;
        }

        public ContentReference Copy()
        {
            return (ContentReference)MemberwiseClone();
        }

        /// <summary>
        /// Determines whether the specified content link is null or empty.
        /// </summary>
        /// <param name="contentLink">The content link.</param>
        /// <returns>
        /// <c>true</c> if content link is null or empty; otherwise, <c>false</c>.
        ///     </returns>
        public static bool IsNullOrEmpty(ContentReference contentLink)
        {
            if (contentLink == null)
                return true;
            if (contentLink._contentId == 0)
                return contentLink._versionId == 0;
            return false;
        }

        /// <summary>Tries the parse.</summary>
        /// <param name="complexReference">The complex reference.</param>
        /// <param name="result">The result.</param>
        /// <returns>
        /// </returns>
        public static bool TryParse(string complexReference, out ContentReference result)
        {
            result = EmptyReference;
            if (string.IsNullOrEmpty(complexReference))
                return complexReference != null;
            if (complexReference == "-")
            {
                result = SelfReference;
                return true;
            }

            var referenceParts = complexReference.Split('_');
            if (referenceParts.Length > 2 || !int.TryParse(referenceParts[0], out var contentId))
                return false;

            if (referenceParts.Length == 1)
            {
                result = new ContentReference(contentId);
                return true;
            }

            var versionId = 0;
            if (referenceParts[1].Length > 0 && !int.TryParse(referenceParts[1], out versionId))
                return false;

            result = new ContentReference(contentId, versionId);
            return true;
        }

        /// <summary>
        /// Parses the specified string to a <see cref="T:EZms.Core.ContentReference" /> instance.
        /// </summary>
        /// <param name="s">The string that should be parsed.</param>
        /// <returns>A <see cref="T:EZms.Core.ContentReference" /> instance if the string could be parsed; otherwise an exception in thrown.</returns>
        public static ContentReference Parse(string s)
        {
            if (!TryParse(s, out var result))
                throw new Exception("ContentReference: Input string was not in a correct format.");
            return result;
        }

        /// <summary>Implements the operator ==.</summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>
        /// Returns true if x.ID == y.ID and x.WorkID == y.WorkID
        /// otherwise false
        /// </returns>
        public static bool operator ==(ContentReference x, ContentReference y)
        {
            return x == y;
        }

        /// <summary>Implements the operator !=.</summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(ContentReference x, ContentReference y)
        {
            return !(x == y);
        }
    }
}