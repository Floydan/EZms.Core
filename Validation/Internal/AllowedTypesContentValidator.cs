using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using EZms.Core.Models;
using EZms.Core.Repositories;

namespace EZms.Core.Validation.Internal
{
    internal class AllowedTypesContentValidator : AllowedTypesValidator<IContent>
    {
        public override ValidationResult IsValid(IContent value, ValidationContext validationContext, IEnumerable<Type> allowedTypes,
            IEnumerable<Type> restrictedTypes)
        {
            if (value == null)
                return null;
            if (restrictedTypes.FirstOrDefault(t => t.IsInstanceOfType(value)) != null || !allowedTypes.Any(t => t.IsInstanceOfType(value)))
                return CreateValidationError(value, validationContext);
            return null;
        }

        private static ValidationResult CreateValidationError(
            IContent content,
            ValidationContext validationContext)
        {
            return new ValidationResult(
                $"Type '{content.ModelType.Name}' is not allowed on property '{(string.IsNullOrEmpty(validationContext.DisplayName) ? validationContext.MemberName : validationContext.DisplayName)}'");
        }
    }
}
