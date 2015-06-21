# SoapParser

This library is just one class to help you generate and parse a [SOAP](http://www.w3.org/TR/soap/) envelope, something I was surprised to see wasn't actually available in the .Net framework as a stand alone class (Or at least I haven't found it).

Its use is very simple. To create a SOAP envelope you create an instance of the class `SoapEnvelope` and assign the `Headers` and `Body` properties (And possible the `Exception` if you want to signal an error) and then call the `ToDocument` method to generate the XML document for the SOAP envelope.

To read data simply call the method `SoapEnvelop.FromStream` or `SoapEnvelope.FromRequest` and it will return the envelope it parsed from the stream or request. It does support handling GZip content encoding from the request.

If you would like to see samples of use check out the `SoapEnvelopeTest` class. Below is a simple round trip sample that creates an envelope and then parses it back.

``
      SoapEnvelope envelope = new SoapEnvelope();
      envelope.Body = new[] { new XElement("BodyElement") };
      envelope.Headers = new[] { new XElement("HeaderElement") };
      XDocument doc = envelope.ToDocument();
      MemoryStream stream = new MemoryStream();
      doc.Save(stream);
      stream.Seek(0, SeekOrigin.Begin);
      SoapEnvelope otherEnvelope = SoapEnvelope.FromStream(stream);
``

[Henrik Johnson](http://www.henrik.org)

[Underscore Research](http://www.underscoreresearch.com/)