using System;

namespace gnoam.engine
{
  using NUnit.Framework;

  [TestFixture]
  public class TestCardinalFilter {

    [Test]
    public void Zero() {
      CardinalFilter cf = new CardinalFilter();
      String str = cf.GenerateTextFromNumber(0);
      Assert.AreEqual(str, "0");
    }

    [Test]
    public void Large() {
      CardinalFilter cf = new CardinalFilter();
      String str = cf.GenerateTextFromNumber(10001);
      Assert.AreEqual(str, "10001");
    }

    [Test]
    public void Nine() {
      CardinalFilter cf = new CardinalFilter();
      String str = cf.GenerateTextFromNumber(9);
      Assert.AreEqual(str, "nine");
    }

    [Test]
    public void Ten() {
      CardinalFilter cf = new CardinalFilter();
      String str = cf.GenerateTextFromNumber(10);
      Assert.AreEqual(str, "10");
    }

    [Test]
    public void Five() {
      CardinalFilter cf = new CardinalFilter();
      String str = cf.GenerateTextFromNumber(5);
      Assert.AreEqual(str, "five");
    }

    [Test]
    public void ThreeAsFloat() {
      CardinalFilter cf = new CardinalFilter();
      String str = cf.GenerateTextFromNumber(3.0f);
      Assert.AreEqual(str, "three");
    }

    [Test]
    public void Negative() {
      CardinalFilter cf = new CardinalFilter();
      String str = cf.GenerateTextFromNumber(-1);
      Assert.AreEqual(str, "-1");
    }

    [Test]
    public void Decimal() {
      CardinalFilter cf = new CardinalFilter();
      String str = cf.GenerateTextFromNumber(0.5f);
      Assert.AreEqual(str, "0.5");
    }

  }
}

