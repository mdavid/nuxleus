/*
// File: AtomSyndication.Generator.cs:
// Author:
//  Sylvain Hellegouarch <sh@defuze.org>
//  M. David Peterson <m.david@3rdandUrban.com>
//
// Copyright � 2007-2011 3rd&Urban, LLC
//
// The code contained in this file is licensed under The MIT License
// Please see http://www.opensource.org/licenses/mit-license.php for specific detail.
*/

using System.Xml;
using System.Xml.Serialization;

namespace Nuxleus.ServiceModel.Types.Atom
{
    public class Generator {
        [XmlAttribute("uri")]
        public string Uri;

        [XmlAttribute("version")]
        public string Version;

        [XmlText]
        public string Text;
    }
}