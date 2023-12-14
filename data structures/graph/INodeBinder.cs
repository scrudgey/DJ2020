
public interface INodeBinder<U> where U : Node<U> {
    public U node { get; set; }
    public void HandleNodeChange();
    public void Bind(U target) {
        node = target;
        node.Bind(HandleNodeChange);
        HandleNodeChange();
    }
}