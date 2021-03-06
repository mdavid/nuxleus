﻿// Copyright 2010 Max Toro Q.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Saxon.Api;

namespace myxsl.net.saxon.modules.request {
   
   sealed class UrlComponents : ExtensionFunctionDefinition {

      readonly XdmSequenceType resultType;
      readonly QName _FunctionName;
      readonly XdmSequenceType[] _ArgumentTypes;

      public override XdmSequenceType[] ArgumentTypes { get { return _ArgumentTypes; } }
      public override QName FunctionName { get { return _FunctionName; } }
      public override int MaximumNumberOfArguments { get { return 2; } }
      public override int MinimumNumberOfArguments { get { return 1; } }

      public UrlComponents() {

         this._FunctionName = new QName(RequestModule.Namespace, "url-components");
         this._ArgumentTypes = new[] { 
            new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_STRING), ' '),
            new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_STRING), ' ')
         };
         this.resultType = new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_STRING), ' ');
      }

      public override ExtensionFunctionCall MakeFunctionCall() {
         return new FunctionCall();
      }

      public override XdmSequenceType ResultType(XdmSequenceType[] ArgumentTypes) {
         return this.resultType;
      }

      #region Nested Types

      class FunctionCall : ExtensionFunctionCall {

         public override IXdmEnumerator Call(IXdmEnumerator[] arguments, DynamicContext context) {

            string components = arguments[0].AsAtomicValues().Single().ToString();

            string result;

            if (arguments.Length == 1) {
               result = RequestModule.UrlComponents(components);
            } else {
               string format = arguments[1].AsAtomicValues().Single().ToString();

               result = RequestModule.UrlComponents(components, format);
            }

            return result.ToXdmItem().GetXdmEnumerator();
         }
      }

      #endregion
   }
}
