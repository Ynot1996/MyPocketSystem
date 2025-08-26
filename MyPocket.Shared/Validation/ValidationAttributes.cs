using System;
using System.ComponentModel.DataAnnotations;
using MyPocket.Shared.Resources;

namespace MyPocket.Shared.Validation
{
    // 本地化 Required
    public class LocalizedRequiredAttribute : RequiredAttribute
    {
        public LocalizedRequiredAttribute(string resourceKey)
        {
            ErrorMessage = new JsonLocalizationService().GetString(resourceKey);
        }
    }

    // 本地化 Range
    public class LocalizedRangeAttribute : RangeAttribute
    {
        public LocalizedRangeAttribute(double minimum, double maximum, string resourceKey)
            : base(minimum, maximum)
        {
            ErrorMessage = new JsonLocalizationService().GetString(resourceKey);
        }
    }

    // 本地化 StringLength
    public class LocalizedStringLengthAttribute : StringLengthAttribute
    {
        public LocalizedStringLengthAttribute(int maximumLength, string resourceKey)
            : base(maximumLength)
        {
            ErrorMessage = new JsonLocalizationService().GetString(resourceKey);
        }
    }

    // (可選) 本地化 RegularExpression
    public class LocalizedRegularExpressionAttribute : RegularExpressionAttribute
    {
        public LocalizedRegularExpressionAttribute(string pattern, string resourceKey)
            : base(pattern)
        {
            ErrorMessage = new JsonLocalizationService().GetString(resourceKey);
        }
    }
}

