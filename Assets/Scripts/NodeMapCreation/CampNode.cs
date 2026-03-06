public class CampNode : NodeMapNode
{
    public override void OnClick()
    {
        CompleteCampNode();
    }
    private void CompleteCampNode()
    {
        PlayerDataManager.Instance.UpdateNodeData(_nodeIndex);
        NodeMapManager.Instance.CompleteCurrentNode();
        NodeMapManager.Instance.InitNodes();
    }
}