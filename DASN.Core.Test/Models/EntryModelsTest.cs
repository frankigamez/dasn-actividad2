using System.Collections.Generic;
using DASN.Core.Models.EntryModels;
using DASN.Core.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DASN.Core.Test.Models
{
    [TestClass]
    public class EntryModelsTest
    {
        [TestMethod]
        public void LoginTest()
        {
            var battery = new List<ValidationItem<Login>>();
            ValidationHelper.RequiredTests().ForEach(requ1 =>
            ValidationHelper.EmailAddressTests().ForEach(email =>
            ValidationHelper.StringLengthTests(25, 8).ForEach(stringLength =>
                {
                    //Email
                    battery.Add(new ValidationItem<Login> {
                        ModelToValidate = new Login { Email = requ1, Password = "aaa" }
                    });
                    battery.Add(new ValidationItem<Login> {
                        ModelToValidate = new Login { Email = email, Password = "aaa" }
                    });

                    //Password
                    battery.Add(new ValidationItem<Login> {
                        ModelToValidate = new Login { Email = "a@a.es", Password = requ1 }
                    });
                    battery.Add(new ValidationItem<Login> {
                        ModelToValidate = new Login { Email = "a@a.es", Password = stringLength }
                    });
                }
            )));
            ValidationHelper.ValidateTests(battery);
        }
    }
}
