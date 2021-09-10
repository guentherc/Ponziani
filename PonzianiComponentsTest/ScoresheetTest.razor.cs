using AngleSharp.Dom;
using Bunit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PonzianiComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonzianiComponentsTest
{
    public partial class ScoresheetTest: BunitTestContext
    {
        public string GetId(IRenderedComponent<Scoresheet> scoresheet)
        {
            var nodes = scoresheet.Nodes;
            Assert.AreEqual(1, nodes.Count());
            if (nodes.Count() == 1) return ((IElement)nodes.First()).Id; else return null;
        }
    }
}
