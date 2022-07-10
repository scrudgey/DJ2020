public interface IDamageReceiver {
    abstract public void TakeDamage<T>(T damage) where T : Damage;
}