﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.Revit;
using System.Xml;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using RevitServices.Transactions;
using RevThread = RevitServices.Threading;

namespace Dynamo.Nodes
{
    public class FunctionWithRevit : Function
    {
        internal ElementsContainer ElementsContainer = new ElementsContainer();

        protected internal FunctionWithRevit(CustomNodeDefinition customNodeDefinition)
            : base(customNodeDefinition)
        { }

        public FunctionWithRevit() { }

        //public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        //{
        //    dynRevitSettings.ElementsContainers.Push(ElementsContainer);
        //    var result = base.Evaluate(args);
        //    dynRevitSettings.ElementsContainers.Pop();
        //    return result;
        //}

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            base.SaveNode(xmlDoc, nodeElement, context);

            if (context == SaveContext.Copy)
                return;

            foreach (var node in ElementsContainer.Nodes)
            {
                var outEl = xmlDoc.CreateElement("InnerNode");
                outEl.SetAttribute("id", node.ToString());

                foreach (var run in ElementsContainer[node])
                {
                    var runEl = xmlDoc.CreateElement("Run");

                    foreach (var id in run)
                    {
                        Element e;
                        if (dynUtils.TryGetElement(id, out e))
                        {
                            var elementStore = xmlDoc.CreateElement("Element");
                            elementStore.InnerText = e.UniqueId;
                            runEl.AppendChild(elementStore);
                        }
                    }

                    outEl.AppendChild(runEl);
                }

                nodeElement.AppendChild(outEl);
            }
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            base.LoadNode(nodeElement);

            ElementsContainer.Clear();

            foreach (XmlNode node in nodeElement.ChildNodes)
            {
                if (node.Name == "InnerNode")
                {
                    var nodeId = new Guid(node.Attributes["id"].Value);
                    var runs = ElementsContainer[nodeId];
                    runs.Clear();

                    foreach (XmlNode run in node.ChildNodes)
                    {
                        if (run.Name == "Run")
                        {
                            var runElements = new List<ElementId>();
                            runs.Add(runElements);

                            var query = from XmlNode element in run.ChildNodes
                                        where element.Name == "Element"
                                        select element.InnerText;

                            foreach (var eid in query) 
                            {
                                try
                                {
                                    runElements.Add(DocumentManager.Instance.CurrentUIDocument.Document.GetElement(eid).Id);
                                }
                                catch (NullReferenceException)
                                {
                                    dynSettings.DynamoLogger.Log("Element with UID \"" + eid + "\" not found in Document.");
                                }
                            }
                        }
                    }
                    //var rNode = Definition.WorkspaceModel.Nodes.FirstOrDefault(x => x.GUID == nodeId) as RevitTransactionNode;
                    //if (rNode != null)
                    //    rNode.RegisterAllElementsDeleteHook();
                }
            }
        }

        public override void Destroy()
        {
            RevThread.IdlePromise.ExecuteOnIdleAsync(
               delegate
               {
                   TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);
                   try
                   {
                       ElementsContainer.DestroyAll();
                   }
                   catch (Exception ex)
                   {
                       dynSettings.DynamoLogger.Log(
                          "Error deleting elements: "
                          + ex.GetType().Name
                          + " -- " + ex.Message);
                   }
                   TransactionManager.Instance.ForceCloseTransaction();
                   WorkSpace.Modified();
               });
        }
    }
}
