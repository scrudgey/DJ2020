public enum KeyType { physical, keycard, password, keycardCode, physicalCode, keypadCode }
public class KeyData {
    public int idn;
    public KeyType type;
    public KeyData(KeyType type, int idn) {
        this.type = type;
        this.idn = idn;
    }
}