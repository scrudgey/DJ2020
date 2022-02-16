public interface IGraphOverlay<out U, out V, out W> where U : Graph<V, U> where V : Node where W : NodeIndicator<V, U> {
}