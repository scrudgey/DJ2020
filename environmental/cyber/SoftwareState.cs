public class SoftwareState {
    public SoftwareTemplate template;
    public int charges;
    public SoftwareState(SoftwareTemplate template) {
        this.template = template;
        this.charges = template.maxCharges;
    }
}