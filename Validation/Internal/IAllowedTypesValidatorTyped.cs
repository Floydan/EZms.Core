using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EZms.Core.Validation.Internal
{
    internal interface IAllowedTypesValidator<in T>
    {
        ValidationResult IsValid(
            T value,
            ValidationContext validationContext,
            IEnumerable<Type> allowedTypes,
            IEnumerable<Type> restrictedTypes);
    }
}
