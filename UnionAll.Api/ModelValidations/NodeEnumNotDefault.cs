using System.ComponentModel.DataAnnotations;
using UnionAll.Domain;

namespace UnionAll.Api.ModelValidations
{
    // a custom attribute validation.
    public class NodeEnumNotDefault: ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string enumName = validationContext.MemberName;
            string valueToValidate = (string)value;
            string enumDefault;

            switch (enumName)
            {
                case (nameof(Node.NodeType)):
                    enumDefault = NodeValueTypes.Default.ToString();
                    break;
                case (nameof(Node.NodeTopic)):
                    enumDefault = NodeTopics.Default.ToString();
                    break;
                default:
                    enumDefault = string.Empty;
                    break;
            }

            if (valueToValidate == enumDefault)
                return new ValidationResult($"{enumName} cannot be updated to Default");

            return ValidationResult.Success;
        }
    }
}
