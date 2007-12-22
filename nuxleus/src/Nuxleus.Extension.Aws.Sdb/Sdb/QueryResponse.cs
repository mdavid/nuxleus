/* -*- Mode: Java; c-basic-offset: 2 -*- */
/*
 * This software code is made available "AS IS" without warranties of any
 * kind.  You may copy, display, modify and redistribute the software
 * code either by itself or as incorporated into your code; provided that
 * you do not remove any proprietary notices.  Your use of this software
 * code is at your own risk and you waive any claim against Amazon
 * Web Services LLC or its affiliates with respect to your use of
 * this software code.
 * 
 * @copyright 2007 Amazon Web Services LLC or its affiliates.
 *            All rights reserved.
 */
using System;
using System.Collections;
using System.Xml;
using Nuxleus.Extension.Aws;

namespace Nuxleus.Extension.Aws.Sdb
{
    public class QueryResponse : ListResponse
    {
        private ArrayList items = new ArrayList();
        private string responseName = "QueryResponse";

        public QueryResponse (IAwsConnection connection, string domainName,
                 string response)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(response);

            CheckErrorResponse(doc);

            processResponse(responseName, doc);

            foreach (XmlNode node in doc.ChildNodes)
            {
                if (node.Name.Equals(responseName))
                {
                    foreach (XmlNode itemNode in node.ChildNodes)
                    {
                        if (itemNode.Name.Equals("ItemName"))
                        {
                            items.Add(new Item(connection, domainName, itemNode.InnerText));
                        }
                        if (itemNode.Name.Equals("MoreToken"))
                        {
                            MoreToken = itemNode.InnerText;
                        }
                    }
                }
            }
        }

        public ICollection Items ()
        {
            return items;
        }
    }
}
