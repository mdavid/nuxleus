﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Globalization;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;

namespace Enyim.Caching.Memcached
{
	internal class GetOperation : ItemOperation
	{
		private static log4net.ILog log = log4net.LogManager.GetLogger(typeof(GetOperation));

		private object result;

		internal GetOperation(ServerPool pool, string key)
			: base(pool, key)
		{
		}

		public object Result
		{
			get { return this.result; }
		}

		protected override bool ExecuteAction()
		{
			this.Socket.SendCommand("get " + this.HashedKey);

			GetResponse r = GetHelper.ReadItem(this.Socket);

			if (r != null)
			{
				this.result = this.ServerPool.Transcoder.Deserialize(r.Item);
				GetHelper.FinishCurrent(this.Socket);
			}

			return true;
		}
	}
}

#region [ License information          ]
/* ************************************************************
 *
 * Copyright (c) Attila Kiskó, enyim.com, 2007
 *
 * This source code is subject to terms and conditions of 
 * Microsoft Permissive License (Ms-PL).
 * 
 * A copy of the license can be found in the License.html
 * file at the root of this distribution. If you can not 
 * locate the License, please send an email to a@enyim.com
 * 
 * By using this source code in any fashion, you are 
 * agreeing to be bound by the terms of the Microsoft 
 * Permissive License.
 *
 * You must not remove this notice, or any other, from this
 * software.
 *
 * ************************************************************/
#endregion