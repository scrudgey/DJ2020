using System;

public enum KeyType { physical, keycard, password, keycardCode, physicalCode, keypadCode }
public class KeyData {
    public int idn;
    public KeyType type;
    public KeyData(KeyType type, int idn) {
        this.type = type;
        this.idn = idn;
    }

    public override bool Equals(object obj) {
        return Equals(obj as KeyData);
    }

    public bool Equals(KeyData other) {
        return other != null &&
               idn == other.idn &&
               type == other.type;
    }

    public override int GetHashCode() {
        return HashCode.Combine(idn, type);
    }
}