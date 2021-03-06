﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Generic;

namespace Sepia.Globalization.Numbers.Rules
{
    [TestClass]
    public class RulesetGroupTest
    {
        RulesetGroup spelloutRules;

        public RulesetGroupTest()
        {
            var xml = Cldr.Instance
                .GetDocuments("common/rbnf/en.xml")
                .FirstElement("ldml/rbnf/rulesetGrouping");
            spelloutRules = RulesetGroup.Parse(xml);
        }

        [TestMethod]
        public void Loading()
        {
            Assert.AreEqual("SpelloutRules", spelloutRules.Type);
            Assert.AreNotEqual(0, spelloutRules.Rulesets.Count);
        }

        [TestMethod]
        public void Formatting()
        {
            Assert.AreEqual("minus one", spelloutRules.Format(-1m, "spellout-numbering"));
            Assert.AreEqual("zero", spelloutRules.Format(0m, "spellout-numbering"));
            Assert.AreEqual("thirteen", spelloutRules.Format(13m, "spellout-numbering"));
            Assert.AreEqual("twenty", spelloutRules.Format(20m, "spellout-numbering"));
            Assert.AreEqual("twenty-one", spelloutRules.Format(21m, "spellout-numbering"));
            Assert.AreEqual("three hundred", spelloutRules.Format(300m, "spellout-numbering"));
            Assert.AreEqual("three hundred twenty-one", spelloutRules.Format(321m, "spellout-numbering"));
            Assert.AreEqual("one thousand two hundred thirty-four", spelloutRules.Format(1234m, "spellout-numbering"));
            Assert.AreEqual("minus one thousand two hundred thirty-four", spelloutRules.Format(-1234m, "spellout-numbering"));
            Assert.AreEqual("twenty quadrillion", spelloutRules.Format(20000000000000000m, "spellout-numbering"));
        }

        [TestMethod]
        public void Formatting_Max_Value()
        {
            Assert.AreEqual("79,228,162,514,264,337,593,543,950,335", spelloutRules.Format(decimal.MaxValue, "spellout-numbering"));
        }

        [TestMethod]
        public void Formatting_Min_Value()
        {
            Assert.AreEqual("minus 79,228,162,514,264,337,593,543,950,335", spelloutRules.Format(decimal.MinValue, "spellout-numbering"));
        }

        [TestMethod]
        public void Formatting_Fractions()
        {
            Assert.AreEqual("one point two", spelloutRules.Format(1.2m, "spellout-numbering"));
            Assert.AreEqual("one point zero two", spelloutRules.Format(1.02m, "spellout-numbering"));
            Assert.AreEqual("one point zero zero zero zero two", spelloutRules.Format(1.00002m, "spellout-numbering"));

            Assert.AreEqual("one point two", spelloutRules.Format(1.2, "spellout-numbering"));
            Assert.AreEqual("one point zero two", spelloutRules.Format(1.02, "spellout-numbering"));
            Assert.AreEqual("one point zero zero zero zero two", spelloutRules.Format(1.00002, "spellout-numbering"));
        }

        [TestMethod]
        public void Formatting_Infinity()
        {
            Assert.AreEqual("infinity", spelloutRules.Format(double.PositiveInfinity, "spellout-numbering"));
            Assert.AreEqual("minus infinity", spelloutRules.Format(double.NegativeInfinity, "spellout-numbering"));
        }

        [TestMethod]
        public void Formatting_NaN()
        {
            Assert.AreEqual("not a number", spelloutRules.Format(double.NaN, "spellout-numbering"));
        }

    }
}