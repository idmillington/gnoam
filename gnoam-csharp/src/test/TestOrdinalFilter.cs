using System;

namespace gnoam.engine
{
  using NUnit.Framework;

  [TestFixture]
  public class TestOrdinalFilter {

    [Test]
    public void Zero() {
      OrdinalFilter of = new OrdinalFilter();
      String zeroStr = of.GenerateTextFromNumber(0);
      Assert.AreEqual(zeroStr, "0th");
    }

    [Test]
    public void Large() {
      OrdinalFilter of = new OrdinalFilter();
      String str = of.GenerateTextFromNumber(10001);
      Assert.AreEqual(str, "10001st");
    }

    [Test]
    public void Nine() {
      OrdinalFilter of = new OrdinalFilter();
      String str = of.GenerateTextFromNumber(9);
      Assert.AreEqual(str, "ninth");
    }

    [Test]
    public void Ten() {
      OrdinalFilter of = new OrdinalFilter();
      String str = of.GenerateTextFromNumber(10);
      Assert.AreEqual(str, "10th");
    }

    [Test]
    public void Five() {
      OrdinalFilter of = new OrdinalFilter();
      String str = of.GenerateTextFromNumber(5);
      Assert.AreEqual(str, "fifth");
    }

    [Test]
    public void ThreeAsFloat() {
      OrdinalFilter of = new OrdinalFilter();
      String str = of.GenerateTextFromNumber(3.0f);
      Assert.AreEqual(str, "third");
    }

    [Test]
    public void Negative() {
      OrdinalFilter of = new OrdinalFilter();
      String str = of.GenerateTextFromNumber(-1);
      Assert.AreEqual(str, "-1");
    }

    [Test]
    public void Decimal() {
      OrdinalFilter of = new OrdinalFilter();
      String str = of.GenerateTextFromNumber(0.5f);
      Assert.AreEqual(str, "0.5");
    }
      
  }
}

