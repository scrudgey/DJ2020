public interface IDamageable {
    abstract public void TakeDamage<T>(T damage) where T : Damage;
}