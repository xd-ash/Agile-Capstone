public class OtherNode : NodeMapNode
{
    public override void OnClick()
    {
        //once true node implemented, add loot get or other event
        PlayerDataManager.Instance.UpdateNodeData(_nodeIndex);
        NodeMapManager.Instance.CompleteCurrentNode();
        NodeMapManager.Instance.InitNodes();
    }
}