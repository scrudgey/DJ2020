public class SoftwareState {
    public SoftwareTemplate template;
    public int charges;
    public SoftwareState(SoftwareTemplate template) {
        this.template = template;
        this.charges = template.maxCharges;
    }
    public bool EvaluateCondition(CyberNode target) {
        bool result = charges > 0;
        foreach (SoftwareCondition condition in template.conditions) {
            result &= condition.Evaluate(target);
        }
        return result;
    }
}