using System;
using System.Collections.Generic;
using DASN.Core.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.DataAnnotations;

namespace DASN.SecurityTest
{
    [TestClass]
    public class EntriesModelValidationTest
    {
        private interface IValidationExample
        {
            string Stub { get; set; }
        }
        private static void RunTest<T>(List<string> expectedInvalidValues, List<string> expectedValidValues) where T : IValidationExample, new()
        {
            var battery = new List<ValidationItem<T>>();
            expectedInvalidValues.ForEach(x =>
                battery.Add(new ValidationItem<T> {ModelToValidate = new T {Stub = x}, ResultExpected = false}));
            expectedValidValues.ForEach(x =>
                battery.Add(new ValidationItem<T> {ModelToValidate = new T {Stub = x}, ResultExpected = true}));
            ValidationHelper.ValidateTests(battery);
        }


        /// <summary>
        /// Testeamos el atributo Required de un modelo
        /// </summary>
        [TestMethod]
        public void Required_Test()
            => RunTest<RequiredValidationExample>(ValidationHelper.RequiredTests(), new List<string> {Guid.NewGuid().ToString()});
        private class RequiredValidationExample : IValidationExample
        {
            [Required]
            public string Stub { get; set; }
        }


        /// <summary>
        /// Testeamos el atributo EmailAddress de un modelo
        /// </summary>        
        [TestMethod]
        public void EmailAddress_Test()
            => RunTest<EmailAddressValidationExample>(ValidationHelper.EmailAddressTests(), new List<string> { "test@test.com" });
        private class EmailAddressValidationExample : IValidationExample
        {
            [EmailAddress]
            public string Stub { get; set; }
        }


        /// <summary>
        /// Testeamos el atributo StringLength de un modelo
        /// </summary>      
        [TestMethod]
        public void StringLength_Test()
        {
            RunTest<StringLengthValidation1Example>(ValidationHelper.StringLengthTests(5), new List<string> { "1", "12", "123", "1234", "12345" });
            RunTest<StringLengthValidation2Example>(ValidationHelper.StringLengthTests(5, 2), new List<string> { "12", "123", "1234", "12345" });
        }
        private class StringLengthValidation1Example : IValidationExample
        {
            [StringLength(5)]
            public string Stub { get; set; }
        }
        private class StringLengthValidation2Example : IValidationExample
        {
            [StringLength(5,MinimumLength = 2)]
            public string Stub { get; set; }
        }


        /// <summary>
        /// Testeamos el atributo Range de un modelo
        /// </summary>        
        [TestMethod]
        public void Range_Test()
            => RunTest<RangeValidationExample>(ValidationHelper.RangeTests(5,10), new List<string> { "5","10", "7"});
        private class RangeValidationExample : IValidationExample
        {
            [Range(5, 10)]
            public string Stub { get; set; }
        }        
    }
}
