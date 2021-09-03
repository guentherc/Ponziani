using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bunit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PonzianiComponentsTest
{
    public abstract class BunitTestContext : TestContextWrapper
    {
        [TestInitialize]
        public void Setup()
        {
            TestContext = new Bunit.TestContext();
            var moduleInterop = TestContext.JSInterop.SetupModule("./_content/PonzianiComponents/ponziani.js");
            moduleInterop.SetupVoid("setHeight", _ => true);
            moduleInterop.SetupVoid("scrollToBottom", _ => true);
        }

        [TestCleanup]
        public void TearDown() => TestContext?.Dispose();
    }
}
