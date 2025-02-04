using FluentValidation;
using System.Net;
using UTG.Exceptions;

namespace Utg.Api.Validators
{
    /// <summary>
    /// Model Validation Extension
    /// </summary>
    public static class ModelValidationExtension
    {
        /// <summary>
        /// Validate Model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="validator"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static void ValidateModel<T>(this IValidator<T> validator, T model)
        {
            var result = validator.Validate(model);
            if (!result.IsValid)
            {
                var errors = result.Errors;
                throw new OpiException(string.Join(",", errors), (int)HttpStatusCode.BadRequest);               
            }
        }
    }
}
