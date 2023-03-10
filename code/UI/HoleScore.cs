using Sandbox;
using Sandbox.UI;

namespace Facepunch.Minigolf.UI;

#pragma warning disable IDE0051, CA1822

[UseTemplate]
public partial class HoleScore : Panel
{
	public string Stroke => $"{ Sandbox.Game.LocalClient.GetPar() }";
	public string Par => $"{ Game.Current.Course.CurrentHole.Par }";
	public string Hole => $"{ Game.Current.Course.CurrentHole.Number }";
}
