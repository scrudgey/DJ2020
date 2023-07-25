using Items;
public class ItemSaleData {
    public ItemTemplate item;
    public int cost;
    public ItemSaleData(ItemTemplate baseItem, int cost) {
        this.item = baseItem;
        this.cost = cost;
    }
}