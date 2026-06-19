namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

internal interface ILayout3CommandGuard
{
    Layout3CommandDecision Evaluate(Layout3CommandIntent intent);
}
