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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Underscore.SoapParser
{
  /// <summary>
  /// SOAP envelope parser.
  /// </summary>
  public class SoapEnvelope
  {
    /// <summary>
    /// This exception is used specifically to indicate an explicit SOAP Fault message.
    /// </summary>
    public class SoapErrorException : Exception
    {
      /// <summary>
      /// The XML element.
      /// </summary>
      public XElement FaultElement { get; private set; }

      public SoapErrorException(XElement fault)
      {
        if (fault.Name != SoapNamespace + "Fault")
          throw new ArgumentException("fault", "The fault element must have the name SoapNamespace:Fault");
        FaultElement = fault;
      }
    }

    /// <summary>
    /// SOAP namespace.
    /// </summary>
    public static readonly XNamespace SoapNamespace = "http://schemas.xmlsoap.org/soap/envelope/";

    /// <summary>
    /// SOAP Header.
    /// </summary>
    public virtual XElement[] Headers { get; set; }

    /// <summary>
    /// SOAP Body.
    /// </summary>
    public virtual XElement[] Body { get; set; }

    /// <summary>
    /// Exception encountered while processing.
    /// </summary>
    public virtual Exception Exception { get; set; }

    /// <summary>
    /// Generate a stram with the XML document contents.
    /// </summary>
    public XDocument ToDocument()
    {
      XElement header = new XElement(SoapNamespace + "Header");
      if (Headers != null)
        header.Add(Headers);

      XElement body = new XElement(SoapNamespace + "Body");
      if (Body != null)
        body.Add(Body);

      if (Exception != null)
      {
        SoapErrorException soapExc = Exception as SoapErrorException;
        if (soapExc != null)
          body.Add(soapExc.FaultElement);
        else
          body.Add(
            new XElement(SoapNamespace + "Fault",
              new XElement(SoapNamespace + "faultcode",
                String.Format("{0}.{1}", Exception.GetType().FullName, Exception.HResult)),
              new XElement(SoapNamespace + "faultstring",
                Exception.Message),
              new XElement(SoapNamespace + "detail", Exception.ToString())));
      }

      XElement envelope = new XElement(SoapNamespace + "Envelope", header, body);
      XDocument doc = new XDocument(envelope);

      return doc;
    }

    /// <summary>
    /// Create SOAP envelope from an HTTP request.
    /// </summary>
    public static SoapEnvelope FromRequest(HttpRequestBase request)
    {
      string contentEncoding = request.Headers["Content-Encoding"];      
      if (contentEncoding == "gzip")
        return FromStream(new GZipStream(request.InputStream, CompressionMode.Decompress));
      return FromStream(request.InputStream);
    }

    /// <summary>
    /// Create a SOAP envelope from a stream.
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static SoapEnvelope FromStream(Stream stream)
    {
      XDocument doc = XDocument.Load(stream);
      XElement root = doc.Root;
      if (root != null && root.Name.Equals(SoapNamespace + "Envelope"))
      {
        SoapEnvelope ret = new SoapEnvelope();

        foreach (XElement element in root.Elements())
        {
          if (element.Name.Equals(SoapNamespace + "Header"))
            ret.Headers = element.Elements().ToArray();
          else if (element.Name.Equals(SoapNamespace + "Body"))
          {
            List<XElement> bodyElements = new List<XElement>();
            foreach (XElement elem in element.Elements())
            {
              if (elem.Name == SoapNamespace + "Fault")
                ret.Exception = new SoapErrorException(elem);
              else
                bodyElements.Add(elem);
            }
            ret.Body = bodyElements.ToArray();
          }
          else
            throw new XmlSchemaException(String.Format("Invalid SOAP Envelope contents: {0}", root.Name));
        }

        return ret;
      }
      throw new XmlSchemaException(String.Format("Invalid SOAP envelope: {0}", root != null ? root.Name : null));
    }
  }
}
