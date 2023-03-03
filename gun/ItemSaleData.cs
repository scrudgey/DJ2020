using Items;
public class ItemSaleData {
    public BaseItem item;
    public int cost;
    public ItemSaleData(BaseItem baseItem, int cost) {
        this.item = baseItem;
        this.cost = cost;
    }
}