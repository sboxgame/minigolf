using Sandbox;
using Sandbox.UI;

namespace Facepunch.Minigolf.UI;

#pragma warning disable IDE0051, CA1822

[UseTemplate]
public partial class HoleScore : Panel
{
	string Stroke => $"{ Local.Client.GetPar() }";
	string Par => $"{ Game.Current.Course.CurrentHole.Par }";
	string Hole => $"{ Game.Current.Course.CurrentHole.Number }";
}