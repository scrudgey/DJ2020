public interface IGraphOverlay<out U, out V, out W> where U : Graph<V, U> where V : Node<V> where W : NodeIndicator<V, U> {
    public OverlayHandler overlayHandler { get; set; }

}