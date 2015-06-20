#region Copyright
//
// Copyright 2015 Underscore Research LLC.  
// ALL RIGHTS RESERVED.
//
// UNDERSCORE RESEARCH LLC. MAKES NO REPRESENTATIONS OR
// WARRANTIES ABOUT THE SUITABILITY OF THE SOFTWARE, EITHER
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE, OR NON-INFRINGEMENT. DELL SHALL
// NOT BE LIABLE FOR ANY DAMAGES SUFFERED BY LICENSEE
// AS A RESULT OF USING, MODIFYING OR DISTRIBUTING
// THIS SOFTWARE OR ITS DERIVATIVES.
//
// Authored by Henrik Johnson
//
#endregion

using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using NUnit.Framework;

namespace Underscore.SoapParser.Test
{
  [TestFixture]
  public class SoapEnvelopeTest
  {
    [Test]
    public void BasicTest()
    {
      SoapEnvelope envelope = new SoapEnvelope();
      envelope.Body = new[] { new XElement("BodyElement") };
      envelope.Headers = new[] { new XElement("HeaderElement") };
      XDocument doc = envelope.ToDocument();
      MemoryStream stream = new MemoryStream();
      doc.Save(stream);
      stream.Seek(0, SeekOrigin.Begin);
      SoapEnvelope otherEnvelope = SoapEnvelope.FromStream(stream);
      Assert.AreEqual(1, otherEnvelope.Body.Length);
      Assert.AreEqual(XName.Get("BodyElement"), otherEnvelope.Body[0].Name);
      Assert.AreEqual(1, otherEnvelope.Headers.Length);
      Assert.AreEqual(XName.Get("HeaderElement"), otherEnvelope.Headers[0].Name);
    }

    [Test]
    public void SampleTest()
    {
      string xml = @"<Envelope xmlns=""http://schemas.xmlsoap.org/soap/envelope/"">
  <Header />
  <Body>
    <Discover xmlns=""urn:schemas-microsoft-com:xml-analysis"">
      <RequestType>MDSCHEMA_DIMENSIONS</RequestType>
      <Restrictions>
        <RestrictionList>
          <CATALOG_NAME>Adventure Works DW 2008R2</CATALOG_NAME>
          <CUBE_NAME>Adventure Works</CUBE_NAME>
        </RestrictionList>
      </Restrictions>
      <Properties>
        <PropertyList>
          <Catalog>Adventure Works DW 2008R2</Catalog>
        </PropertyList>
      </Properties>
    </Discover>
  </Body>
</Envelope>";
      MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
      SoapEnvelope envelope = SoapEnvelope.FromStream(stream);
      Assert.AreEqual(0, envelope.Headers.Length);
      Assert.AreEqual(1, envelope.Body.Length);
      XNamespace ns = "urn:schemas-microsoft-com:xml-analysis";
      Assert.AreEqual(ns + "Discover", envelope.Body[0].Name);
    }

    [Test]
    public void FaultParsing()
    {
      SoapEnvelope envelope = new SoapEnvelope();
      envelope.Body = new[] { new XElement("BodyElement") };
      envelope.Headers = new[] { new XElement("HeaderElement") };

      try
      {
        throw new Exception("Failed");
      }
      catch (Exception exc)
      {
        envelope.Exception = exc;
      }

      XDocument doc = envelope.ToDocument();
      MemoryStream stream = new MemoryStream();
      doc.Save(stream);
      stream.Seek(0, SeekOrigin.Begin);

      byte[] firstStream = stream.ToArray();

      SoapEnvelope otherEnvelope = SoapEnvelope.FromStream(stream);
      Assert.AreEqual(1, otherEnvelope.Body.Length);
      Assert.AreEqual(XName.Get("BodyElement"), otherEnvelope.Body[0].Name);
      Assert.AreEqual(1, otherEnvelope.Headers.Length);
      Assert.AreEqual(XName.Get("HeaderElement"), otherEnvelope.Headers[0].Name);
      Assert.IsNotNull(otherEnvelope.Exception);

      stream = new MemoryStream();
      doc = otherEnvelope.ToDocument();
      doc.Save(stream);
      Assert.AreEqual(0, new ByteArrayComparer().Compare(firstStream, stream.ToArray()));
    }
  }
}
