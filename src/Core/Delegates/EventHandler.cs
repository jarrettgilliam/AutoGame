namespace AutoGame.Core.Delegates;

public delegate void EventHandler<in TSender, in TArgs>(TSender sender, TArgs e);
