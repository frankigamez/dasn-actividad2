using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace DASN.Core.Test.Helpers
{
    internal class ValidationHelper 
    {
        internal static void ValidateTest<T>(ValidationItem<T> validationItemItem)
        {
            var context = new ValidationContext(validationItemItem.ModelToValidate, null, null);
            var result = new List<ValidationResult>();
            var flagResult = Validator.TryValidateObject(validationItemItem.ModelToValidate, context, result, true);

            if (flagResult != validationItemItem.ResultExpected)
                Assert.Fail(
                    $" IN: {JsonConvert.SerializeObject(validationItemItem.ModelToValidate)} ->" +
                    $" EXPECTED RESULT: {validationItemItem.ResultExpected} ->" +
                    $" RESULT: {flagResult} ->" +
                    $" MESSAGE: {string.Join(", ", result.Select(x => $"[{x.MemberNames} - {x.ErrorMessage}]"))}");            
        }

        internal static void ValidateTests<T>(List<ValidationItem<T>> entryTestItems) => entryTestItems.ForEach(ValidateTest);

        internal static List<string> RequiredTests() => new List<string>{null, "", " "};
        internal static List<string> EmailAddressTests() => new List<string>{"12345", "test@", "@test", "test@test", "tes@test,", "test@test.", "test@test.1", "test@test,com"};
        internal static List<string> StringLengthTests(int maximumLength, int? minimumLength = null)
        {
            string GetText(int length)
            {
                var sb = new StringBuilder();
                for (var i = 0; i < length; i++)
                    sb.Append((char)new Random().Next(65, 90));
                return sb.ToString();
            }
            var result = new List<string> { GetText(maximumLength + 1) };
            if (minimumLength.HasValue)
                result.Add(GetText(minimumLength.Value-1));
            return result;
        }
        internal static List<string> RangeTests(int from, int to) => new List<string>{ (from - 1).ToString(), (to +1).ToString()};
    }

    internal class ValidationItem<T>
    {
        public T ModelToValidate { get; set; }
        public bool ResultExpected { get; set; }

        public ValidationItem()
        {
            ResultExpected = false;
        }
    }   
}