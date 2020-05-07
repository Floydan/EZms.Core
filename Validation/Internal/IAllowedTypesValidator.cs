using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EZms.Core.Validation.Internal
{
    internal interface IAllowedTypesValidator
    {
        bool CanValidate(object value);

        ValidationResult IsValid(
            object value,
            ValidationContext validationContext,
            IEnumerable<Type> allowedTypes,
            IEnumerable<Type> restrictedTypes);
    }
}
