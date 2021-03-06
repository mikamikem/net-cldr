﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Generic;

namespace Sepia.Globalization
{

    [TestClass]
    public class LocaleIdentifierTest
    {
        [TestMethod]
        public void Parsing_Language()
        {
            var id = LocaleIdentifier.Parse("en");
            Assert.AreEqual("en", id.Language);

            Assert.IsFalse(LocaleIdentifier.TryParse("NotALanguageCode", out id));
        }

        [TestMethod]
        public void Parsing_Language_Country()
        {
            var id = LocaleIdentifier.Parse("en-US");
            Assert.AreEqual("en", id.Language);
            Assert.AreEqual("US", id.Region);

            Assert.IsFalse(LocaleIdentifier.TryParse("en-NotACountryCode", out id));
        }

        [TestMethod]
        public void Parsing_Language_Region()
        {
            var id = LocaleIdentifier.Parse("es-419");
            Assert.AreEqual("es", id.Language);
            Assert.AreEqual("419", id.Region);

            Assert.IsFalse(LocaleIdentifier.TryParse("es-12345", out id));
        }

        [TestMethod]
        public void Parsing_Language_Script()
        {
            var id = LocaleIdentifier.Parse("uz-Cyrl");
            Assert.AreEqual("uz", id.Language);
            Assert.AreEqual("Cyrl", id.Script);

            Assert.IsFalse(LocaleIdentifier.TryParse("es-NotScript", out id));
        }

        [TestMethod]
        public void Parsing_Language_Variant()
        {
            var id = LocaleIdentifier.Parse("sl-nedis");
            Assert.AreEqual("sl", id.Language);
            Assert.IsTrue(id.Variants.Contains("nedis"));
        }

        [TestMethod]
        public void Parsing_Language_Region_Variant()
        {
            var id = LocaleIdentifier.Parse("de-CH-1996");
            Assert.AreEqual("de", id.Language);
            Assert.AreEqual("CH", id.Region);
            Assert.IsTrue(id.Variants.Contains("1996"));
        }

        [TestMethod]
        public void Parsing_Language_Region_Multiple_Variants()
        {
            var id = LocaleIdentifier.Parse("de-CH-1996-1901");
            Assert.AreEqual("de", id.Language);
            Assert.AreEqual("CH", id.Region);
            Assert.IsTrue(id.Variants.Contains("1996"));
            Assert.IsTrue(id.Variants.Contains("1901"));
        }

        [TestMethod]
        public void Parsing_Language_Script_Country()
        {
            var id = LocaleIdentifier.Parse("zh-Hant-CN");
            Assert.AreEqual("zh", id.Language);
            Assert.AreEqual("Hant", id.Script);
            Assert.AreEqual("CN", id.Region);

            Assert.IsFalse(LocaleIdentifier.TryParse("zh-Hant-NotACountry", out id));
        }

        [TestMethod]
        public void Parsing_Script()
        {
            var id = LocaleIdentifier.Parse("Cyrl");
            Assert.AreEqual("Cyrl", id.Script);

            Assert.IsFalse(LocaleIdentifier.TryParse("NotAScript", out id));
        }

        [TestMethod]
        public void Parsing_Script_Region()
        {
            var id = LocaleIdentifier.Parse("Cyrl-RU");
            Assert.AreEqual("Cyrl", id.Script);
            Assert.AreEqual("RU", id.Region);

            Assert.IsFalse(LocaleIdentifier.TryParse("Cyrl-NotACounty", out id));
        }

        [TestMethod]
        public void Parsing_Underscore_Separator()
        {
            var id = LocaleIdentifier.Parse("zh_Hant_CN");
            Assert.AreEqual("zh", id.Language);
            Assert.AreEqual("Hant", id.Script);
            Assert.AreEqual("CN", id.Region);
        }

        [TestMethod]
        public void Parsing_Extension()
        {
            var id = LocaleIdentifier.Parse("en-Latn-GB-r-extended-sequence-r-foo");
            Assert.AreEqual("en", id.Language);
            Assert.AreEqual("Latn", id.Script);
            Assert.AreEqual("GB", id.Region);
            Assert.IsTrue(id.Extensions.Contains("r-extended-sequence"));
            Assert.IsTrue(id.Extensions.Contains("r-foo"));
        }

        [TestMethod]
        public void Parsing_Unicode_Extension()
        {
            var id = LocaleIdentifier.Parse("th-u-bar-foo-ca-buddhist-kk-nu-thai");
            var extension = id.UnicodeExtension;
            CollectionAssert.Contains(extension.Attributes.ToArray(), "foo");
            CollectionAssert.Contains(extension.Attributes.ToArray(), "bar");
            Assert.AreEqual("buddhist", extension.Keywords["ca"]);
            Assert.AreEqual("true", extension.Keywords["kk"]);
            Assert.AreEqual("thai", extension.Keywords["nu"]);
        }

        [TestMethod]
        public void Parsing_Invalid_Unicode_Extension()
        {
            LocaleIdentifier id;
            string message;
            var ok = LocaleIdentifier.TryParse("th-u-xx-invalidtype", out id, out message);
            Assert.IsFalse(ok);
            Assert.IsNull(id);
        }

        [TestMethod]
        public void Parsing_Throws()
        {
            ExceptionAssert.Throws<FormatException>(() => LocaleIdentifier.Parse("ThisIsNotALanguage"));
        }

        [TestMethod]
        public void Parsing_Messages()
        {
            string message;
            LocaleIdentifier id;
            var ok = LocaleIdentifier.TryParse("en-r-alpha-r-alpha", out id, out message);
            Assert.AreEqual(false, ok);
            Assert.AreEqual(null, id);
            Assert.AreEqual("'en-r-alpha-r-alpha' is not a valid locale identifier because an extension is duplicated.", message);
        }

        [TestMethod]
        public void Parsing_Variants_Are_Not_Repeated()
        {
            string message;
            LocaleIdentifier id;
            var ok = LocaleIdentifier.TryParse("de-CH-1901-1901", out id, out message);
            Assert.AreEqual(false, ok);
            Assert.AreEqual(null, id);
            Assert.AreEqual("'de-CH-1901-1901' is not a valid locale identifier because a variant is duplicated.", message);

            ExceptionAssert.Throws<FormatException>(() => LocaleIdentifier.Parse("de-CH-1901-1901"));
        }

        [TestMethod]
        public void Empty_Tags()
        {
            var id = LocaleIdentifier.Parse("Cyrl");
            Assert.AreEqual("", id.Language);
            Assert.AreEqual("Cyrl", id.Script);
            Assert.AreEqual("", id.Region);
            Assert.AreEqual(0, id.Extensions.Count());
            Assert.AreEqual(0, id.Variants.Count());

            id = LocaleIdentifier.Parse("en");
            Assert.AreEqual("en", id.Language);
            Assert.AreEqual("", id.Script);
            Assert.AreEqual("", id.Region);
            Assert.AreEqual(0, id.Extensions.Count());
            Assert.AreEqual(0, id.Variants.Count());
        }

        [TestMethod]
        public void Parsing_Transforms()
        {
            Assert.AreEqual("en-US", LocaleIdentifier.Parse("en-US").ToString());
            Assert.AreEqual("root", LocaleIdentifier.Parse("und").ToString());
            Assert.AreEqual("und-US", LocaleIdentifier.Parse("und-US").ToString());
            Assert.AreEqual("root-u-cu-usd", LocaleIdentifier.Parse("und-u-cu-USD").ToString());
            Assert.AreEqual("zh-TW", LocaleIdentifier.Parse("cmn-TW").ToString());
            Assert.AreEqual("sr-RS", LocaleIdentifier.Parse("sr-CS").ToString());
            Assert.AreEqual("sr-Latn", LocaleIdentifier.Parse("sh").ToString());
            Assert.AreEqual("sr-Cyrl", LocaleIdentifier.Parse("sh-Cyrl").ToString());
            Assert.AreEqual("hy-AM", LocaleIdentifier.Parse("hy-SU").ToString());
        }

        [TestMethod]
        public void Parsing_Normalisation()
        {
            Assert.AreEqual("en-US", LocaleIdentifier.Parse("eng-840").ToString());
            Assert.AreEqual("en-NZ", LocaleIdentifier.Parse("eng-NZ").ToString());
            Assert.AreEqual("en-GB", LocaleIdentifier.Parse("en-UK").ToString());
            Assert.AreEqual("en-GB", LocaleIdentifier.Parse("eng-UK").ToString());
        }

        [TestMethod]
        public void Formatting()
        {
            var id = LocaleIdentifier.Parse("EN_LATN_GB_R_EXTENDED_SEQUENCE_R-FOO");
            Assert.AreEqual("en-Latn-GB-r-extended-sequence-r-foo", id.ToString());

            id = LocaleIdentifier.Parse("EN_nz");
            Assert.AreEqual("en-NZ", id.ToString());

            id = LocaleIdentifier.Parse("EN");
            Assert.AreEqual("en", id.ToString());
        }

        [TestMethod]
        public void UnicodeLangugeTag()
        {
            var id = LocaleIdentifier.Parse("EN_LATN_GB_R_EXTENDED_SEQUENCE_R-FOO");
            Assert.AreEqual("en_Latn_GB", id.ToUnicodeLanguage());

            id = LocaleIdentifier.Parse("EN_nz");
            Assert.AreEqual("en_NZ", id.ToUnicodeLanguage());

            id = LocaleIdentifier.Parse("EN");
            Assert.AreEqual("en", id.ToUnicodeLanguage());

            id = LocaleIdentifier.Parse("DE-ch-1996");
            Assert.AreEqual("de_CH_1996", id.ToUnicodeLanguage());

            id = LocaleIdentifier.Parse("sl-nedis");
            Assert.AreEqual("sl_nedis", id.ToUnicodeLanguage());

            id = LocaleIdentifier.Parse("sl-nedis");
            Assert.AreEqual("sl_NEDIS", id.ToUnicodeLanguage(true));
        }

        [TestMethod]
        public void MostLikely()
        {
            Assert.AreEqual("en-Latn-US", LocaleIdentifier.Parse("en").MostLikelySubtags().ToString());
            Assert.AreEqual("zh-Hans-CN", LocaleIdentifier.Parse("zh").MostLikelySubtags().ToString());
            Assert.AreEqual("zh-Hant-TW", LocaleIdentifier.Parse("zh-TW").MostLikelySubtags().ToString());
            Assert.AreEqual("zh-Hans-SG", LocaleIdentifier.Parse("ZH-ZZZZ-SG").MostLikelySubtags().ToString());
        }

        [TestMethod]
        public void RemoveMostLikely()
        {
            Assert.AreEqual("zh-TW", LocaleIdentifier.Parse("zh_Hant").RemoveMostLikelySubtags().ToString());
            Assert.AreEqual("en", LocaleIdentifier.Parse("en-Latn-US").RemoveMostLikelySubtags().ToString());
        }

        [TestMethod]
        public void SearchChain()
        {
            var id = LocaleIdentifier.Parse("sl-SI-nedis");
            var chain = id.SearchChain().ToArray();
            Assert.AreEqual(5, chain.Length);
            Assert.AreEqual("sl_SI_NEDIS", chain[0]);
            Assert.AreEqual("sl_NEDIS", chain[1]);
            Assert.AreEqual("sl_SI", chain[2]);
            Assert.AreEqual("sl", chain[3]);
            Assert.AreEqual("root", chain[4]);

            id = LocaleIdentifier.Parse("en-Latn-US-basiceng");
            chain = id.SearchChain().ToArray();
            Assert.AreEqual("en_Latn_US_BASICENG", chain[0]);
            Assert.AreEqual("en_US_BASICENG", chain[1]);
            Assert.AreEqual("en_Latn_BASICENG", chain[2]);
            Assert.AreEqual("en_BASICENG", chain[3]);
            Assert.AreEqual("en_Latn_US", chain[4]);
            Assert.AreEqual("en_US", chain[5]);
            Assert.AreEqual("en_Latn", chain[6]);
            Assert.AreEqual("en", chain[7]);
            Assert.AreEqual("root", chain[8]);
        }

        [TestMethod]
        public void SearchChain_und()
        {
            var id = LocaleIdentifier.Parse("und");
            var chain = id.SearchChain().ToArray();
            Assert.AreEqual(1, chain.Length);
            Assert.AreEqual("root", chain[0]);
        }

        [TestMethod]
        public void SearchChain_root()
        {
            var id = LocaleIdentifier.Parse("root");
            var chain = id.SearchChain().ToArray();
            Assert.AreEqual(1, chain.Length);
            Assert.AreEqual("root", chain[0]);
        }

        [TestMethod]
        public void SearchChain_Unicode_Variant_Extension()
        {
            var id = LocaleIdentifier.Parse("en-u-va-posix");
            var chain = id.SearchChain().ToArray();
            Assert.AreEqual(3, chain.Length);
            Assert.AreEqual("en_POSIX", chain[0]);
            Assert.AreEqual("en", chain[1]);
            Assert.AreEqual("root", chain[2]);
        }

        [TestMethod]
        public void CanonicalForm()
        {
            Assert.AreEqual("zh-Hant-TW", LocaleIdentifier.Parse("cmn-TW").CanonicalForm().ToString());
            Assert.AreEqual("en-Latn-GB", LocaleIdentifier.Parse("eng-Latn-GB").CanonicalForm().ToString());
        }
    }
}