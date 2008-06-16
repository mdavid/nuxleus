﻿using System.Collections.Generic;
using System.Text;
using Nuxleus.Extension.AWS.SimpleDB;
using VVMF.SOA.Common;
using EeekSoft.Asynchronous;
using System.Net;
using System.Configuration;
using System.Threading;
using System.Runtime.Remoting;
using System.Collections;
using System.IO;
using Nuxleus.Extension.AWS.SimpleDB.Model;
using System.Xml.Linq;

namespace Nuxleus.Extension.Aws {

    public struct Agent {

        static LoggerScope logger = new LoggerScope();
        static ExceptionHandlerScope exShield = new ExceptionHandlerScope();
        static ProfilerScope profiler = new ProfilerScope();
        static int m_workers = (int.Parse(ConfigurationManager.AppSettings["WorkerQueueMultiplier"]) * System.Environment.ProcessorCount);

        public void Initialize() {

            ServicePointManager.DefaultConnectionLimit = int.Parse(ConfigurationManager.AppSettings["DefaultConnectionLimit"]);

            int minWorkerThreads = int.Parse(ConfigurationManager.AppSettings["MinimumWorkerThreads"]);
            int minAsyncIOThreads = int.Parse(ConfigurationManager.AppSettings["MinimumAsyncIOThreds"]);
            int maxWorkerThreads = int.Parse(ConfigurationManager.AppSettings["MaximumWorkerThreads"]);
            int maxAsyncIOThreads = int.Parse(ConfigurationManager.AppSettings["MaximumAsyncIOThreads"]);

            ThreadPool.SetMaxThreads(maxWorkerThreads, maxAsyncIOThreads);
            ThreadPool.SetMinThreads(minWorkerThreads, minAsyncIOThreads);
        }

        public void Invoke<T>() {
            Scope scope = new Scope();
            scope += profiler.Scope;
            scope += logger.Scope;
            scope += exShield.Scope;

            logger.Message = "Processing SOAP requests";


            scope.Begin = () => {
                using (WorkerQueue q = new WorkerQueue(m_workers)) {
                    for (int i = 0; i < m_workers; i++) {
                        List<string> lines = new List<string>();
                        using (StreamReader csvReader = new StreamReader("AD.txt", Encoding.UTF8, true)) {
                            string inputLine;
                            while ((inputLine = csvReader.ReadLine()) != null) {
                                lines.Add(inputLine);
                            }
                            int runningTotal = 0;
                            List<string> operation = new List<string>();
                            for (int l = 0; l < lines.Count; l++) {
                                int length = lines[l].Length;
                                string thisLine = lines[l];

                                if (runningTotal >= 100) {
                                    runningTotal = 0;
                                    q.EnqueueTask(InvokeOperation<T>(operation));
                                    operation = new List<string>();
                                }
                                operation.Add(thisLine);
                                runningTotal += length;
                            }

                        }
                    }
                }
            };

        }


        private static IEnumerable<IAsync> InvokeOperation<T>(List<string> operation) {
            Dictionary<XElement, T> responseList = new Dictionary<XElement, T>();
            IEnumerable<IAsync>[] processList = new IEnumerable<IAsync>[operation.Count];
            int i = 0;
            foreach (string inputLine in operation) {
                string[] inputArray = inputLine.Split(new char[] { '\u0009' });
                processList[i] = CreateTask<T>(inputArray).Invoke(responseList);
                i++;
            }
            yield return Async.Parallel(processList);
            int c = 1;
            IEnumerator responseEnumerator = responseList.GetEnumerator();
            while (responseEnumerator.MoveNext()) {
                KeyValuePair<XElement, XElement> responseItem = (KeyValuePair<XElement, XElement>)responseEnumerator.Current;
                System.Console.WriteLine(".......................... Begin Message {0} ............................", c);
                System.Console.WriteLine("\n");
                System.Console.WriteLine("[Message {0} Sent]", c);
                responseItem.Key.Save(System.Console.Out);
                System.Console.WriteLine("\n");
                System.Console.WriteLine("[Message {0} Received]", c);
                responseItem.Value.Save(System.Console.Out);
                System.Console.WriteLine("\n");
                System.Console.WriteLine(".......................... End Message {0} ............................", c);
                c++;
            }
        }

        private static PutAttributes CreateTask<T>(string[] inputArray) {
            System.Console.WriteLine(System.String.Format("Loading Item: {0}, with Place Name: {1}", (string)inputArray.GetValue(0), (string)inputArray.GetValue(1)));
            System.Console.WriteLine(System.String.Format("Array Length: {0}", inputArray.Length));

            KeyValuePair<string, string>[] geoNames = new KeyValuePair<System.String, System.String>[] { 
                            new KeyValuePair<string,string>("geonamesid",(string)inputArray.GetValue(0)),
                            new KeyValuePair<string,string>("names",(string)inputArray.GetValue(1)),
                            new KeyValuePair<string,string>("alternatenames",(string)inputArray.GetValue(3)), 
                            new KeyValuePair<string,string>("latitude", (string)inputArray.GetValue(4)),
                            new KeyValuePair<string,string>("longitude", (string)inputArray.GetValue(5)),
                            new KeyValuePair<string,string>("feature_class", (string)inputArray.GetValue(6)),
                            new KeyValuePair<string,string>("feature_code",(string)inputArray.GetValue(7)),
                            new KeyValuePair<string,string>("country_code",(string)inputArray.GetValue(8)),
                            new KeyValuePair<string,string>("cc2",(string)inputArray.GetValue(9)),
                            new KeyValuePair<string,string>("admin1_code",(string)inputArray.GetValue(10)),
                            new KeyValuePair<string,string>("admin2_code",(string)inputArray.GetValue(11)),
                            new KeyValuePair<string,string>("admin3_code",(string)inputArray.GetValue(12)),
                            new KeyValuePair<string,string>("admin4_code",(string)inputArray.GetValue(13)),
                            new KeyValuePair<string,string>("population",(string)inputArray.GetValue(14)),
                            new KeyValuePair<string,string>("elevation",(string)inputArray.GetValue(15)),
                            new KeyValuePair<string,string>("gtopo30",(string)inputArray.GetValue(16)),
                            new KeyValuePair<string,string>("timezone",(string)inputArray.GetValue(17)),
                            new KeyValuePair<string,string>("modification_date",(string)inputArray.GetValue(18)),
                        };


            IEnumerator attributeArray = geoNames.GetEnumerator();

            ArrayList attributes = new ArrayList();

            while (attributeArray.MoveNext()) {
                KeyValuePair<System.String, System.String> attribute = (KeyValuePair<System.String, System.String>)attributeArray.Current;
                string title = attribute.Key;
                string current = attribute.Value.Normalize().ToLower();

                if (current.Length > 0) {
                    if (current.Contains(",")) {
                        IEnumerator csvEnumerator = current.Split(new char[] { ',' }).GetEnumerator();
                        while (csvEnumerator.MoveNext()) {
                            attributes.Add(new Attribute(title, (string)csvEnumerator.Current));
                        }
                    } else {
                        attributes.Add(new Attribute(title, current));
                    }
                }
            }
            return new PutAttributes { DomainName = "foobar", ItemName = (string)inputArray.GetValue(0), AttributeArray = attributes };
        }
    }
}
