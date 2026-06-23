using System.Collections.Generic;
using System.Linq;

namespace UtilLibs;

public static class TechUtils
{
	public static void AddNode(ResourceTreeLoader<ResourceTreeNode> tech_tree_nodes_instance, string newTechId, string previousNode, float xDiff, float yDiff)
	{
		AddNode(tech_tree_nodes_instance, newTechId, new string[1] { previousNode }, xDiff, yDiff);
	}

	public static void AddNode(ResourceTreeLoader<ResourceTreeNode> tech_tree_nodes_instance, string newTechId, string[] previousNodes, float xDiff, float yDiff)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		xDiff *= 350f;
		yDiff *= 250f;
		ResourceTreeNode val = null;
		List<ResourceTreeNode> list = new List<ResourceTreeNode>();
		foreach (ResourceTreeNode item2 in (ResourceLoader<ResourceTreeNode>)(object)tech_tree_nodes_instance)
		{
			if (previousNodes.Count() > 0 && ((Resource)item2).Id == previousNodes.First())
			{
				val = item2;
			}
			if (previousNodes.Contains(((Resource)item2).Id))
			{
				list.Add(item2);
			}
		}
		if (val == null)
		{
			return;
		}
		ResourceTreeNode item = new ResourceTreeNode
		{
			height = val.height,
			width = val.width,
			nodeX = val.nodeX + xDiff,
			nodeY = val.nodeY + yDiff,
			edges = new List<Edge>(val.edges),
			references = new List<ResourceTreeNode>(),
			Disabled = false,
			Id = newTechId,
			Name = newTechId
		};
		foreach (ResourceTreeNode item3 in list)
		{
			item3.references.Add(item);
		}
		((ResourceLoader<ResourceTreeNode>)(object)tech_tree_nodes_instance).resources.Add(item);
	}

	public static void AddNode(ResourceTreeLoader<ResourceTreeNode> tech_tree_nodes_instance, string newTechId, string previousNode, string x_coord_ref_techNode, string y_coord_ref_techNode)
	{
		AddNode(tech_tree_nodes_instance, newTechId, new string[1] { previousNode }, x_coord_ref_techNode, y_coord_ref_techNode);
	}

	public static void AddNode(ResourceTreeLoader<ResourceTreeNode> tech_tree_nodes_instance, string newTechId, string[] previousNodes, string x_coord_ref_techNode, string y_coord_ref_techNode)
	{
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Expected O, but got Unknown
		ResourceTreeNode val = null;
		float nodeX = 0f;
		float nodeY = 0f;
		List<ResourceTreeNode> list = new List<ResourceTreeNode>();
		foreach (ResourceTreeNode item2 in (ResourceLoader<ResourceTreeNode>)(object)tech_tree_nodes_instance)
		{
			if (previousNodes.Count() > 0 && ((Resource)item2).Id == previousNodes.First())
			{
				val = item2;
				Debug.Log((object)("X: " + item2.nodeX + ", Y: " + item2.nodeY));
			}
			if (previousNodes.Contains(((Resource)item2).Id))
			{
				list.Add(item2);
			}
			if (((Resource)item2).Id == y_coord_ref_techNode)
			{
				nodeY = item2.nodeY;
				Debug.Log((object)("X: " + item2.nodeX + ", Y: " + item2.nodeY));
			}
			else if (((Resource)item2).Id == x_coord_ref_techNode)
			{
				nodeX = item2.nodeX;
				Debug.Log((object)("X: " + item2.nodeX + ", Y: " + item2.nodeY));
			}
		}
		if (val != null)
		{
			ResourceTreeNode item = new ResourceTreeNode
			{
				height = val.height,
				width = val.width,
				nodeX = nodeX,
				nodeY = nodeY,
				edges = new List<Edge>(val.edges),
				references = list,
				Disabled = false,
				Id = newTechId,
				Name = newTechId
			};
			((ResourceLoader<ResourceTreeNode>)(object)tech_tree_nodes_instance).resources.Add(item);
		}
	}
}
