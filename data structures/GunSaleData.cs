public class GunSaleData {
    public GunTemplate template;
    public int cost;
    public string sellerDescription;
    public GunSaleData(GunTemplate template, int cost) {
        this.template = template;
        this.cost = cost;
        this.sellerDescription = "I found this on the street.";
    }
}