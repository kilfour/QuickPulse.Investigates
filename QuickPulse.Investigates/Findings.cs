using QuickPulse.Arteries;

namespace QuickPulse.Investigates;

public class Findings : TheCollector<string>
{
    public bool AllEqual => TheExhibit.Count == 0;
    public string Report => string.Join(Environment.NewLine, TheExhibit);
}
