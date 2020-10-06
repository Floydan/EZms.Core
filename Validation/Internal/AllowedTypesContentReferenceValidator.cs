using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EZms.Core.Loaders;
using EZms.Core.Models;

namespace EZms.Core.Validation.Internal
{
    internal class AllowedTypesContentReferenceValidator : AllowedTypesValidator<ContentReference>
    {
        private readonly IContentLoader _contentLoader;
        private readonly IAllowedTypesValidator<IContent> _contentValidator;

        public AllowedTypesContentReferenceValidator(
            IContentLoader contentLoader,
            IAllowedTypesValidator<IContent> contentValidator)
        {
            _contentLoader = contentLoader;
            _contentValidator = contentValidator;
        }

        public override ValidationResult IsValid(
            ContentReference value, 
            ValidationContext validationContext, 
            IEnumerable<Type> allowedTypes,
            IEnumerable<Type> restrictedTypes)
        {
            if (value == null)
                return null;
            return _contentLoader.TryGet<IContent>(value.Id, out var content) ? 
                _contentValidator.IsValid(content, validationContext, allowedTypes, restrictedTypes) : 
                null;
        }
    }
}
