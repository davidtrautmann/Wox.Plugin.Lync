using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Wox.Plugin;

namespace Wox.Plugin.Lync.Test
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void emptyQuery()
        {
            Lync lyncPlugin = new Lync();
            Query query = new Query(String.Empty);
            List<Result> results = lyncPlugin.Query(query);

            Assert.AreEqual(results.Count, 0);
        }
    }
}