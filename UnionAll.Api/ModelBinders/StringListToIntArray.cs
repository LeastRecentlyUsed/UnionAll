using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DataFork.API.ModelBinders
{
    public class StringListToIntArray : IModelBinder
    {
        // Model Bindings in MVC are used to convert the incoming HTTP request object into an
        // associated C# class model.  
        // This custom model binder takes a stting list of identifiers (e.g. from NodeSet or a 
        // VectorSet) and covnerts them to an array of integers that can then be used in a controller 
        // action.
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            // if the binding is not on an enumerable type then fail binding and return the async.
            if (!bindingContext.ModelMetadata.IsEnumerableType)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            // the binding context provides the model (c# class) we are binding to and the input values.
            var stringList = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).ToString();

            // if the input values are null or white space we have a 'bad request' but not a fail.
            if (string.IsNullOrWhiteSpace(stringList))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            // determine the type argument being covnerted to (in this case 'int').
            // note: GetTypeInfo() is from 'system.reflection'
            var elementType = bindingContext.ModelType.GetTypeInfo().GenericTypeArguments[0];

            // create a converter (built-in c# type converter from 'systesm.ComponentModel')
            var typeConverter = TypeDescriptor.GetConverter(elementType);

            // now convert the string list to an enumerable of converted objects (int)
            var convertedValues = stringList.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => typeConverter.ConvertFromString(x.Trim()))
                .ToArray();

            // create the final array to be returned.
            var intArray = Array.CreateInstance(elementType, convertedValues.Length);
            convertedValues.CopyTo(intArray, 0);

            // set the array as the binding context model (return model)
            bindingContext.Model = intArray;

            // return the successful binding result.
            bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
            return Task.CompletedTask;
        }
    }
}
