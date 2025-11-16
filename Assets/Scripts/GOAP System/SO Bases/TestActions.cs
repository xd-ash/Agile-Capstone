using UnityEngine;
public class TestAction : ActionBase, IAction
{
    public TestAction()
    {
        actionName = "action1";
    }

    public bool PostPerform()
    {
        throw new System.NotImplementedException();
    }

    public bool PrePerform()
    {
        throw new System.NotImplementedException();
    }
}
public class Test2Action : ActionBase, IAction
{
    public Test2Action()
    {
        actionName = "action2";
    }

    public bool PostPerform()
    {
        throw new System.NotImplementedException();
    }

    public bool PrePerform()
    {
        throw new System.NotImplementedException();
    }
}
public class Test3Action : ActionBase, IAction
{
    public Test3Action()
    {
        actionName = "action3";
    }

    public bool PostPerform()
    {
        throw new System.NotImplementedException();
    }

    public bool PrePerform()
    {
        throw new System.NotImplementedException();
    }
}
