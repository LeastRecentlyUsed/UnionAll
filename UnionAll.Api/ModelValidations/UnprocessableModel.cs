using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace UnionAll.Api.ModelValidations
{
    // a custom model validation
    public class UnprocessableModel: ObjectResult
    {
        public UnprocessableModel(ModelStateDictionary modelState)
            : base(new SerializableError(modelState))
        {
            if (modelState == null)
                throw new ArgumentNullException(nameof(modelState));

            StatusCode = 422;
        }
    }
}
