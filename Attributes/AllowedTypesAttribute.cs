using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EZms.Core.Helpers;
using EZms.Core.Models;
using EZms.Core.Validation.Internal;

namespace EZms.Core.Attributes
{
    /// <summary>
    /// Assigns types that the property accept or restrict adding.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AllowedTypesAttribute : ValidationAttribute
    {
        private Type[] _restrictedTypes = Array.Empty<Type>();
        private Type[] _allowedTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:AllowedTypesAttribute" /> class.
        /// </summary>
        public AllowedTypesAttribute() : this(typeof(IContentData))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:AllowedTypesAttribute" /> class.
        /// </summary>
        /// <param name="allowedTypes">The allowed types. The null value is replaced with IContentData.</param>
        public AllowedTypesAttribute(params Type[] allowedTypes)
        {
            AllowedTypes = allowedTypes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:AllowedTypesAttribute" /> class.
        /// </summary>
        /// <param name="allowedTypes">The allowed types. The null value is replaced with IContentData.</param>
        /// <param name="restrictedTypes">The restricted types</param>
        public AllowedTypesAttribute(Type[] allowedTypes, Type[] restrictedTypes) : this(allowedTypes)
        {
            RestrictedTypes = restrictedTypes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:AllowedTypesAttribute" /> class.
        /// </summary>
        /// <param name="allowedTypes">The allowed types. The null value is replaced with IContentData.</param>
        /// <param name="restrictedTypes">The restricted types</param>
        /// <param name="typesFormatSuffix">The TypesFormatSuffix</param>
        public AllowedTypesAttribute(
            Type[] allowedTypes,
            Type[] restrictedTypes,
            string typesFormatSuffix) : this(allowedTypes, restrictedTypes)
        {
            TypesFormatSuffix = typesFormatSuffix;
        }

        /// <summary>Gets or sets the allowed types.</summary>
        /// <remarks>
        ///   <para>Setting value null is treated as all <see cref="T:EZms.Core.IContentData" /> types are allowed.</para>
        ///   <para>
        ///     <see cref="P:EZms.Core.AllowedTypesAttribute.RestrictedTypes" /> has presence over <see cref="P:EZms.Core.AllowedTypesAttribute.AllowedTypes" /> meaning that if the instance is assignable to any type in <see cref="P:EZms.Core.AllowedTypesAttribute.RestrictedTypes" /> then will it not be allowed
        ///       regardless if the type is assignable to any type in <see cref="P:EZms.Core.AllowedTypesAttribute.AllowedTypes" />.
        ///       </para>
        /// </remarks>
        /// <value>The allowed types.</value>
        public Type[] AllowedTypes
        {
            get => (Type[])_allowedTypes.Clone();
            set
            {
                Type[] typeArray = value ?? new[] { typeof(IContentData) };
                _allowedTypes = typeArray;
            }
        }

        /// <summary>
        /// Gets types which the property is not allowed to contain.
        /// </summary>
        /// <remarks>
        /// <see cref="P:EZms.Core.AllowedTypesAttribute.RestrictedTypes" /> has presence over <see cref="P:EZms.Core.AllowedTypesAttribute.AllowedTypes" /> meaning that if the instance is assignable to any type in <see cref="P:EZms.Core.AllowedTypesAttribute.RestrictedTypes" /> then will it not be allowed
        ///     regardless if the type is assignable to any type in <see cref="P:EZms.Core.AllowedTypesAttribute.AllowedTypes" />.
        ///     </remarks>
        public Type[] RestrictedTypes
        {
            get => (Type[])_restrictedTypes.Clone();
            set => _restrictedTypes = value ?? Array.Empty<Type>();
        }

        /// <summary>Gets or sets types format suffix.</summary>
        /// <value>The types format suffix.</value>
        public string TypesFormatSuffix { get; set; }



        /// <inheritdoc />
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return (ValidationResult)null;
            //foreach (IAllowedTypesValidator service in AllowedTypesValidators.Services)
            //{
            //    if (service.CanValidate(value))
            //        return service.IsValid(value, validationContext, AllowedTypes, RestrictedTypes);
            //}
            return (ValidationResult)null;
        }
    }
}
