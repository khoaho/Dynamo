﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.DSEngine;
using NUnit.Framework;

namespace Dynamo.Tests
{
    class LibraryCustomizationTests : UnitTestBase
    {
        [Test]
        [Category("Failing")]
        public void CanLoadValidLibraryCustomization()
        {
            var fn = Path.Combine(GetTestDirectory(), @"core/library/ProtoGeometry.dll");

            var c = LibraryCustomizationServices.GetForAssembly(fn);
            Assert.NotNull(c);

            var cat = c.GetNamespaceCategory("Autodesk.DesignScript.Geometry");
            Assert.AreEqual("Geometry", cat);
        }
    }
}
