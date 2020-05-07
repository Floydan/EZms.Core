using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EZms.Core.Validation.Internal
{
    internal abstract class AllowedTypesValidator<T> : IAllowedTypesValidator, IAllowedTypesValidator<T>
        where T : class
    {
        public virtual bool CanValidate(object value)
        {
            return value is T;
        }

        public abstract ValidationResult IsValid(
            T value,
            ValidationContext validationContext,
            IEnumerable<Type> allowedTypes,
            IEnumerable<Type> restrictedTypes);

        ValidationResult IAllowedTypesValidator.IsValid(
            object value,
            ValidationContext validationContext,
            IEnumerable<Type> allowedTypes,
            IEnumerable<Type> restrictedTypes)
        {
            if (value is T obj)
                return IsValid(obj, validationContext, allowedTypes, restrictedTypes);
            return null;
        }
    }
}
