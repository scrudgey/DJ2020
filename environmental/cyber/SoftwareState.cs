using System.Collections.Generic;
public class SoftwareState {
    public SoftwareTemplate template;
    public int charges;
    public SoftwareState(SoftwareTemplate template) {
        this.template = template;
        this.charges = template.maxCharges;
    }
    public bool EvaluateCondition(CyberNode target, CyberNode origin, List<CyberNode> path) {
        bool result = template.infiniteCharges || charges > 0;
        foreach (SoftwareCondition condition in template.conditions) {
            result &= condition.Evaluate(target, origin, path);
        }
        return result;
    }
}