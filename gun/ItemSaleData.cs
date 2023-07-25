using Items;
public class ItemSaleData {
    public ItemInstance item;
    public int cost;
    public ItemSaleData(ItemInstance baseItem, int cost) {
        this.item = baseItem;
        this.cost = cost;
    }
}