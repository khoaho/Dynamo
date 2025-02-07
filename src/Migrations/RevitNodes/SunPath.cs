﻿using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class SunPathDirection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            // Create nodes
            var vectorAsPoint = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(vectorAsPoint, "ProtoGeometry.dll",
                "Vector.AsPoint", "Vector.AsPoint");
            migrationData.AppendNode(vectorAsPoint);

            XmlElement sunPathNode = MigrationManager.CloneAndChangeName(
                oldNode, "DSRevitNodesUI.SunPathDirection", "SunPath Direction");
            sunPathNode.SetAttribute("guid", System.Guid.NewGuid().ToString());
            sunPathNode.SetAttribute("x", (System.Convert.ToDouble(oldNode.GetAttribute("x")) - 230).ToString());
            
            migrationData.AppendNode(sunPathNode);

            // Update connectors
            data.CreateConnector(sunPathNode, 0, vectorAsPoint, 0);

            return migrationData;
        }
    }
}
