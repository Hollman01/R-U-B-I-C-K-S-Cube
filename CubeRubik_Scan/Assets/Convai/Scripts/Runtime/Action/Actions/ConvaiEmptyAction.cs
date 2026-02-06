namespace Convai.Scripts.Action.Actions
{
    public class ConvaiEmptyAction : ConvaiActionBase
    {
        public override void PerformAction() => SetState(ActionState.Completed);
    }
}
