//
// deletemessage.cs: Client API for the bucker queue system
//
// Author:
//   Sylvain Hellegouarch (sh@defuze.org)
//
// Copyright (C) 2007, Sylvain Hellegouarch
// 

using System;
using System.Xml;
using System.IO;
using System.Text;

namespace Nuxleus.Bucker
{
  public class DeleteMessage : Message {
    public DeleteMessage() : base() {
      this.Op = Operation.DeleteMessage;
    }

    public DeleteMessage(IMessage m) : base(m) {
    }

    public override string Serialize() {
      MemoryStream ms = new MemoryStream();
      XmlWriter xw = XmlWriter.Create(ms, this.settings);
      string xml = null;
	
      try {
	xw.WriteStartElement("qs", QS_NS);
	xw.WriteAttributeString("xmlns", null, QS_NS);

	if(this.Type == MessageType.Request){
	  xw.WriteAttributeString("type", "request");
	} else if (this.Type == MessageType.Response){
	  xw.WriteAttributeString("type", "response");
	} 

	if((this.RequestId != null) && (this.RequestId != String.Empty)){
	  xw.WriteAttributeString("req", this.RequestId);
	}

	if((this.ResponseId != null) && (this.ResponseId != String.Empty)){
	  xw.WriteAttributeString("req", this.ResponseId);
	}

	xw.WriteStartElement("op", QS_NS);
	if(this.Force == true){
	  xw.WriteAttributeString("force", "yes");
	}
	xw.WriteStartElement("delete-message", QS_NS);
	xw.WriteEndElement(); // </delete-message>
	xw.WriteEndElement(); // </op>
	if(this.QueueId != null) {
	  xw.WriteElementString("qid", QS_NS, this.QueueId);
	}
	if(this.MessageId != null) {
	  xw.WriteElementString("mid", QS_NS, this.MessageId);
	}
	xw.WriteEndElement(); // </qs>
	xw.Flush();
      } finally {
	if(xw != null) {
	  StreamReader sr = new StreamReader(ms);
	  ms.Seek(0, SeekOrigin.Begin);
	  xml = sr.ReadToEnd();
	  ms.Close();
	  xw.Close();
	}
      }

      return xml;
    }
  }
}
